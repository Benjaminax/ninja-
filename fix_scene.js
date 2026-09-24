const fs = require('fs');
const path = require('path');

// Fix script for SampleScene.unity
// Fixes:
// 1. PolygonCollider2D m_Paths to match new m_Vertices
// 2. CompositeCollider2D stale path data
// 3. Ensures all collider data is consistent

const SCENE_PATH = path.join(__dirname, 'Assets', 'Scenes', 'SampleScene.unity');

console.log('=== Scene Fix Script ===');
const content = fs.readFileSync(SCENE_PATH, 'utf-8');
const lines = content.split('\n');
console.log(`Scene: ${lines.length} lines`);

const newLines = [];

let i = 0;
let fixCount = 0;

while (i < lines.length) {
  const line = lines[i];
  
  // === FIX 1: PolygonCollider2D m_Points section ===
  // Look for the m_Points: inside PolygonCollider2D
  // We need to detect we're inside PolygonCollider2D context
  if (line.trim() === 'm_Points:' && i > 0) {
    // Check if this is inside a PolygonCollider2D by looking back
    let isPolyCollider = false;
    for (let j = i - 1; j >= Math.max(0, i - 50); j--) {
      if (lines[j].includes('PolygonCollider2D:')) { isPolyCollider = true; break; }
      if (lines[j].startsWith('---')) break;
    }
    
    if (isPolyCollider) {
      console.log(`Fix 1: PolygonCollider2D m_Points at line ${i + 1}`);
      
      // Write corrected m_Points with matching vertices and paths
      newLines.push('    m_Points:');
      newLines.push('      m_Vertices:');
      newLines.push('      - {x: -17, y: -17}');
      newLines.push('      - {x: 45, y: -17}');
      newLines.push('      - {x: 45, y: 13}');
      newLines.push('      - {x: -17, y: 13}');
      newLines.push('    m_Paths:');
      newLines.push('    - - {x: -17, y: -17}');
      newLines.push('      - {x: 45, y: -17}');
      newLines.push('      - {x: 45, y: 13}');
      newLines.push('      - {x: -17, y: 13}');
      
      // Skip old m_Points section until we find the next field
      i++;
      while (i < lines.length) {
        const l = lines[i];
        // Stop when we find a line that's not part of m_Points data
        if (l.startsWith('---') || 
            (l.match(/^  m_/) && !l.includes('m_Points') && !l.includes('m_Vertices') && !l.includes('m_Paths')) ||
            (l.match(/^    m_/) && !l.includes('m_Points') && !l.includes('m_Vertices') && !l.includes('m_Paths') && !l.trim().startsWith('-') && !l.trim().startsWith('x:') && !l.trim().startsWith('y:'))) {
          break;
        }
        i++;
      }
      fixCount++;
      continue; // Don't increment i again
    }
  }
  
  // === FIX 2: CompositeCollider2D m_ColliderPaths ===
  if (line.trim() === 'm_ColliderPaths:') {
    // Check if inside CompositeCollider2D
    let isCompositeCollider = false;
    for (let j = i - 1; j >= Math.max(0, i - 50); j--) {
      if (lines[j].includes('CompositeCollider2D:')) { isCompositeCollider = true; break; }
      if (lines[j].startsWith('---')) break;
    }
    
    if (isCompositeCollider) {
      console.log(`Fix 2: CompositeCollider2D m_ColliderPaths at line ${i + 1}`);
      
      // Write empty collider paths - Unity will regenerate
      newLines.push('  m_ColliderPaths: []');
      
      // Skip old m_ColliderPaths data until m_CompositePaths
      i++;
      while (i < lines.length) {
        const l = lines[i];
        if (l.trim().startsWith('m_CompositePaths:') || l.startsWith('---')) break;
        i++;
      }
      fixCount++;
      continue;
    }
  }
  
  // === FIX 3: CompositeCollider2D m_CompositePaths ===
  if (line.trim().startsWith('m_CompositePaths:')) {
    let isCompositeCollider = false;
    for (let j = i - 1; j >= Math.max(0, i - 200); j--) {
      if (lines[j].includes('CompositeCollider2D:')) { isCompositeCollider = true; break; }
      if (lines[j].startsWith('---')) break;
    }
    
    if (isCompositeCollider) {
      console.log(`Fix 3: CompositeCollider2D m_CompositePaths at line ${i + 1}`);
      
      // Write empty composite paths - Unity will regenerate
      newLines.push('  m_CompositePaths:');
      newLines.push('    m_Paths: []');
      
      // Skip old m_CompositePaths data
      i++;
      while (i < lines.length) {
        const l = lines[i];
        if ((l.match(/^  m_/) && !l.includes('m_Paths') && !l.trim().startsWith('-')) || l.startsWith('---')) break;
        i++;
      }
      fixCount++;
      continue;
    }
  }
  
  // No fix needed, copy line as-is
  newLines.push(line);
  i++;
}

console.log(`\nTotal fixes applied: ${fixCount}`);
console.log(`Output: ${newLines.length} lines`);

// Write fixed file
fs.writeFileSync(SCENE_PATH, newLines.join('\n'), 'utf-8');
console.log('✅ Scene file fixed and saved!');

// Verify key sections
const verifyContent = fs.readFileSync(SCENE_PATH, 'utf-8');
const verifyLines = verifyContent.split('\n');

// Find and show PolygonCollider2D m_Points
for (let j = 0; j < verifyLines.length; j++) {
  if (verifyLines[j].trim() === 'm_Points:') {
    let isPolyC = false;
    for (let k = j - 1; k >= Math.max(0, j - 50); k--) {
      if (verifyLines[k].includes('PolygonCollider2D:')) { isPolyC = true; break; }
      if (verifyLines[k].startsWith('---')) break;
    }
    if (isPolyC) {
      console.log('\n=== PolygonCollider2D m_Points (verified) ===');
      for (let k = j; k < Math.min(j + 12, verifyLines.length); k++) {
        console.log(k + 1, ':', verifyLines[k]);
      }
    }
  }
}

// Check for NaN values
const nanLines = verifyLines.filter(l => l.includes('NaN'));
console.log(`\nLines with NaN: ${nanLines.length}`);
if (nanLines.length > 0) {
  nanLines.forEach(l => console.log('  NaN found:', l.trim()));
}

console.log('\n✅ Done! Reopen Unity to test.');
