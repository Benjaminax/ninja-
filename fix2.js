const fs = require('fs');
const scenePath = 'Assets/Scenes/SampleScene.unity';
const content = fs.readFileSync(scenePath, 'utf-8');
const lines = content.split('\n');
console.log('Lines:', lines.length);

const newLines = [];
let i = 0;
let fixed = false;

while (i < lines.length) {
  const line = lines[i];
  
  // Find PolygonCollider2D m_Points
  if (line === '  m_Points:' || line === '    m_Points:') {
    let isPolyColl = false;
    for (let j = i - 1; j >= Math.max(0, i - 40); j--) {
      if (lines[j] === 'PolygonCollider2D:') { isPolyColl = true; break; }
      if (lines[j].startsWith('---')) break;
    }
    
    if (isPolyColl) {
      console.log('Found PolygonCollider2D m_Points at line', i + 1);
      
      // Write proper format (from backup analysis)
      newLines.push('  m_Points:');
      newLines.push('    m_Paths:');
      newLines.push('    - - {x: -17, y: -17}');
      newLines.push('      - {x: 45, y: -17}');  
      newLines.push('      - {x: 45, y: 13}');
      newLines.push('      - {x: -17, y: 13}');
      
      // Skip old data until m_UseDelaunayMesh line
      i++;
      while (i < lines.length) {
        if (lines[i].includes('m_UseDelaunayMesh')) {
          break;
        }
        if (lines[i].startsWith('---')) {
          break;
        }
        i++;
      }
      fixed = true;
      continue; // don't increment, process current line normally
    }
  }
  
  newLines.push(line);
  i++;
}

console.log('Fixed:', fixed);
console.log('New lines:', newLines.length);
fs.writeFileSync(scenePath, newLines.join('\n'), 'utf-8');
console.log('Done!');
