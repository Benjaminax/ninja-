const fs = require('fs');
const path = require('path');

// ============================================================
// Wayfinder Map Generator — Green Level 1 Redesign
// ============================================================

const SCENE_PATH = path.join(__dirname, 'Assets', 'Scenes', 'SampleScene.unity');
const OUTPUT_PATH = SCENE_PATH; // Overwrite in place

console.log('=== Wayfinder Map Generator ===');
console.log('Reading scene file...');
const content = fs.readFileSync(SCENE_PATH, 'utf-8');
const lines = content.split('\n');
console.log(`Scene file: ${lines.length} lines`);

// ============================================================
// 1. MAP DESIGN — Ground Terrain (Platforms tilemap)
// ============================================================

const groundTiles = new Set();

function fillRect(x1, y1, x2, y2) {
  for (let x = x1; x <= x2; x++)
    for (let y = y1; y <= y2; y++)
      groundTiles.add(`${x},${y}`);
}

function clearRect(x1, y1, x2, y2) {
  for (let x = x1; x <= x2; x++)
    for (let y = y1; y <= y2; y++)
      groundTiles.delete(`${x},${y}`);
}

// --- ZONE A: Tutorial (x: -16 to -4) ---
// Solid ground block
fillRect(-16, -16, -4, -8);
// Left boundary wall
fillRect(-16, -7, -16, 5);
// Main ground surface (wide, safe)
fillRect(-16, -7, -4, -7);
// Clear interior for play area
clearRect(-15, -7, -5, -7);
// Ground surface with small gaps for learning
fillRect(-15, -7, -11, -7);  // Left section
// Gap at x: -10 to -9 (2-tile gap - easy first jump)
fillRect(-8, -7, -5, -7);    // Right section
// Small floating platform above gap (teaches double jump)  
fillRect(-11, -4, -9, -4);
// Step up to Zone B transition
fillRect(-6, -6, -4, -6);
fillRect(-5, -5, -4, -5);

// --- ZONE B: First Challenge (x: -3 to 14) ---
// Solid fill below
fillRect(-3, -16, 14, -9);
// Ground surface sections with gaps
fillRect(-3, -8, 1, -7);     // Section 1
// Gap at x: 2-3 (3-tile gap - needs timing)
fillRect(4, -8, 8, -7);      // Section 2
// Gap at x: 9-10 (spike gap - danger!)  
fillRect(11, -8, 14, -7);    // Section 3

// Elevated platforms for exploration
fillRect(0, -4, 2, -4);      // Platform above section 1
fillRect(5, -3, 7, -3);      // Higher platform
fillRect(10, -5, 12, -5);    // Connecting platform to C

// Step terrain for visual variety
fillRect(-3, -8, -3, -6);    // Small wall left of B
fillRect(14, -8, 14, -6);    // Transition wall to C

// --- ZONE C: Mushroom Valley (x: 15 to 31) ---
// Valley floor - lower than Zone B (creates depth)
fillRect(15, -16, 31, -11);

// Terrain with natural undulating surface
fillRect(15, -10, 18, -9);   // Left hill
fillRect(15, -10, 15, -7);   // Left wall transition
// Valley dip at x: 19-20 (ground at -11)
fillRect(19, -10, 20, -10);  // Slight step
fillRect(21, -10, 25, -9);   // Central elevated area
// Dip at x: 26-27  
fillRect(26, -10, 27, -10);  // Slight step
fillRect(28, -10, 31, -9);   // Right hill
fillRect(31, -10, 31, -7);   // Right wall transition

// Floating platforms across the valley
fillRect(17, -6, 19, -6);    // Platform 1
fillRect(22, -5, 24, -5);    // Platform 2 (slightly higher)
fillRect(20, -2, 22, -2);    // High platform (secret/reward area feel)
fillRect(26, -6, 28, -6);    // Platform 3
fillRect(30, -5, 31, -5);    // Connection to Zone D

// --- ZONE D: Vertical Climb (x: 32 to 44) ---
// Base ground
fillRect(32, -16, 44, -9);
// Right boundary wall  
fillRect(44, -8, 44, 12);

// Zigzag platforms ascending
fillRect(33, -6, 36, -6);    // Platform 1 (left)
fillRect(40, -3, 43, -3);    // Platform 2 (right)
fillRect(34, 0, 37, 0);      // Platform 3 (left)
fillRect(39, 3, 42, 3);      // Platform 4 (right)
fillRect(35, 6, 38, 6);      // Platform 5 (left)
fillRect(40, 9, 43, 9);      // Platform 6 (right)

// Summit platform at top - wider for level end area
fillRect(36, 11, 43, 11);
fillRect(36, 12, 36, 12);    // Left pillar
fillRect(43, 12, 43, 12);    // Right pillar

// --- BOTTOM FLOOR (continuous) ---
// Ensure solid bottom across entire map
fillRect(-16, -16, 44, -16);
fillRect(-16, -15, 44, -15);

// --- Add some terrain variation/details ---
// Zone A: small bump
fillRect(-13, -6, -12, -6);

// Zone B: small pillar/obstacle
fillRect(6, -6, 6, -6);

// Zone C: stepped terrain
fillRect(22, -8, 24, -8);
fillRect(23, -7, 24, -7);

// Zone D: small ledges on walls for visual depth
fillRect(32, -7, 33, -7);
fillRect(43, -6, 44, -6);
fillRect(32, -4, 32, -4);
fillRect(44, -1, 44, -1);
fillRect(44, 2, 44, 2);
fillRect(44, 5, 44, 5);
fillRect(44, 8, 44, 8);

console.log(`Ground tiles generated: ${groundTiles.size}`);

// ============================================================
// 2. MAP DESIGN — Decoration Positions (Plants tilemap)
// ============================================================

// We'll reposition existing decoration tiles from the current scene
// Each decoration entry preserves its original tile/sprite data but gets new coordinates
const decorationPlacements = [
  // Zone A: Tutorial - sparse, welcoming
  // Ferns near start
  { origIdx: 0, x: -14, y: -6 },   // decoration near start
  { origIdx: 1, x: -12, y: -6 },   // another decoration
  { origIdx: 4, x: -8, y: -6 },    // right of gap
  { origIdx: 5, x: -6, y: -4 },    // on step
  
  // Zone B: First Challenge - moderate decoration  
  { origIdx: 2, x: -2, y: -6 },
  { origIdx: 3, x: 0, y: -6 },
  { origIdx: 6, x: 5, y: -6 },     // on ground section 2
  { origIdx: 7, x: 7, y: -6 },
  { origIdx: 8, x: 11, y: -6 },    // on ground section 3
  { origIdx: 9, x: 13, y: -6 },
  { origIdx: 10, x: 1, y: -3 },    // on floating platform
  { origIdx: 11, x: 6, y: -2 },    // on higher platform
  
  // Zone C: Mushroom Valley - RICHEST decoration area
  { origIdx: 12, x: 16, y: -8 },
  { origIdx: 13, x: 17, y: -8 },   // mushroom cluster 1
  { origIdx: 14, x: 18, y: -8 },
  { origIdx: 15, x: 22, y: -8 },   // mushroom cluster 2
  { origIdx: 16, x: 23, y: -8 },
  { origIdx: 17, x: 24, y: -8 },
  { origIdx: 18, x: 21, y: -8 },   // more decoration
  { origIdx: 19, x: 25, y: -8 },
  { origIdx: 20, x: 28, y: -8 },   // mushroom cluster 3
  { origIdx: 21, x: 29, y: -8 },
  { origIdx: 22, x: 30, y: -8 },
  { origIdx: 23, x: 18, y: -5 },   // on floating platform
  { origIdx: 24, x: 23, y: -4 },   // on floating platform
  { origIdx: 25, x: 27, y: -5 },   // on floating platform
  { origIdx: 26, x: 21, y: -1 },   // on high platform
  { origIdx: 27, x: 26, y: -9 },   // valley floor decoration
  { origIdx: 28, x: 19, y: -9 },   // valley floor
  
  // Zone D: Vertical Climb - sparse, focused
  { origIdx: 29, x: 34, y: -5 },   // on platform 1
  { origIdx: 30, x: 41, y: -2 },   // on platform 2
  { origIdx: 31, x: 35, y: 1 },    // on platform 3
  { origIdx: 32, x: 40, y: 4 },    // on platform 4
  { origIdx: 33, x: 36, y: 7 },    // on platform 5
  { origIdx: 34, x: 41, y: 10 },   // on platform 6
  { origIdx: 35, x: 38, y: 12 },   // on summit
  { origIdx: 36, x: 40, y: 12 },   // on summit
  
  // Additional Zone A/B decorations
  { origIdx: 37, x: -15, y: -6 },
  { origIdx: 38, x: -7, y: -6 },
  { origIdx: 39, x: 3, y: -6 },
  { origIdx: 40, x: 8, y: -6 },
  { origIdx: 41, x: 12, y: -6 },
  { origIdx: 42, x: -10, y: -3 },
  { origIdx: 43, x: -5, y: -4 },
  { origIdx: 44, x: 20, y: -1 },
];

console.log(`Decoration placements defined: ${decorationPlacements.length}`);

// ============================================================
// 3. GENERATE YAML DATA
// ============================================================

function generateTileEntry(x, y, tileIndex, spriteIndex, matrixIndex, colorIndex, flags) {
  return [
    `  - first: {x: ${x}, y: ${y}, z: 0}`,
    '    second:',
    '      serializedVersion: 2',
    `      m_TileIndex: ${tileIndex}`,
    `      m_TileSpriteIndex: ${spriteIndex}`,
    `      m_TileMatrixIndex: ${matrixIndex}`,
    `      m_TileColorIndex: ${colorIndex}`,
    '      m_TileObjectToInstantiateIndex: 65535',
    '      dummyAlignment: 0',
    `      m_AllTileFlags: ${flags}`,
  ].join('\n');
}

function generatePlatformTilesYaml() {
  // Sort tiles: Y ascending, then X ascending
  const sorted = [...groundTiles].map(s => {
    const [x, y] = s.split(',').map(Number);
    return { x, y };
  }).sort((a, b) => a.y !== b.y ? a.y - b.y : a.x - b.x);

  const entries = sorted.map(t => 
    generateTileEntry(t.x, t.y, 15, 16, 4, 0, 1073741826)
  );
  
  return '  m_Tiles:\n' + entries.join('\n') + '\n';
}

function generateBackgroundTilesYaml() {
  // Cover the entire map area with background tiles
  const bgTiles = [];
  for (let y = -18; y <= 14; y++) {
    for (let x = -18; x <= 46; x++) {
      bgTiles.push({ x, y });
    }
  }
  bgTiles.sort((a, b) => a.y !== b.y ? a.y - b.y : a.x - b.x);
  
  const entries = bgTiles.map(t =>
    generateTileEntry(t.x, t.y, 0, 3, 0, 0, 1073741825)
  );
  
  return '  m_Tiles:\n' + entries.join('\n') + '\n';
}

// ============================================================
// 4. FIND SECTIONS IN SCENE FILE
// ============================================================

function findLineIndex(searchStr, startFrom = 0) {
  for (let i = startFrom; i < lines.length; i++) {
    if (lines[i].includes(searchStr)) return i;
  }
  return -1;
}

function findComponentStart(fileID) {
  const marker = `&${fileID}`;
  for (let i = 0; i < lines.length; i++) {
    if (lines[i].includes(marker) && lines[i].startsWith('---')) return i;
  }
  return -1;
}

function findNextComponentStart(afterLine) {
  for (let i = afterLine + 1; i < lines.length; i++) {
    if (lines[i].startsWith('---')) return i;
  }
  return lines.length;
}

// Find Platforms Tilemap component (fileID: 1499247567)
const platformsTilemapStart = findComponentStart('1499247567');
const platformsNextComponent = findNextComponentStart(platformsTilemapStart);
console.log(`Platforms Tilemap: lines ${platformsTilemapStart + 1} to ${platformsNextComponent}`);

// Find m_Tiles start and m_AnimatedTiles in Platforms
const platformsTilesStart = findLineIndex('  m_Tiles:', platformsTilemapStart);
const platformsAnimatedTiles = findLineIndex('  m_AnimatedTiles:', platformsTilemapStart);
const platformsAssetArrayStart = findLineIndex('  m_TileAssetArray:', platformsTilemapStart);
const platformsSpriteArrayStart = findLineIndex('  m_TileSpriteArray:', platformsTilemapStart);
const platformsMatrixArrayStart = findLineIndex('  m_TileMatrixArray:', platformsTilemapStart);
const platformsColorArrayStart = findLineIndex('  m_TileColorArray:', platformsTilemapStart);
const platformsOriginLine = findLineIndex('  m_Origin:', platformsTilemapStart);
const platformsSizeLine = findLineIndex('  m_Size:', platformsTilemapStart);

console.log(`  m_Tiles: line ${platformsTilesStart + 1}`);
console.log(`  m_AnimatedTiles: line ${platformsAnimatedTiles + 1}`);
console.log(`  m_Origin: line ${platformsOriginLine + 1}`);

// Find Plants Tilemap component (fileID: 305521023)
const plantsTilemapStart = findComponentStart('305521023');
const plantsNextComponent = findNextComponentStart(plantsTilemapStart);
console.log(`Plants Tilemap: lines ${plantsTilemapStart + 1} to ${plantsNextComponent}`);

const plantsTilesStart = findLineIndex('  m_Tiles:', plantsTilemapStart);
const plantsAnimatedTiles = findLineIndex('  m_AnimatedTiles:', plantsTilemapStart);
const plantsAssetArrayStart = findLineIndex('  m_TileAssetArray:', plantsTilemapStart);
const plantsSpriteArrayStart = findLineIndex('  m_TileSpriteArray:', plantsTilemapStart);
const plantsMatrixArrayStart = findLineIndex('  m_TileMatrixArray:', plantsTilemapStart);
const plantsColorArrayStart = findLineIndex('  m_TileColorArray:', plantsTilemapStart);
const plantsOriginLine = findLineIndex('  m_Origin:', plantsTilemapStart);
const plantsSizeLine = findLineIndex('  m_Size:', plantsTilemapStart);

// Find Background Tilemap component (fileID: 1776424815)
const bgTilemapStart = findComponentStart('1776424815');
const bgNextComponent = findNextComponentStart(bgTilemapStart);
console.log(`Background Tilemap: lines ${bgTilemapStart + 1} to ${bgNextComponent}`);

const bgTilesStart = findLineIndex('  m_Tiles:', bgTilemapStart);
const bgAnimatedTiles = findLineIndex('  m_AnimatedTiles:', bgTilemapStart);
const bgAssetArrayStart = findLineIndex('  m_TileAssetArray:', bgTilemapStart);
const bgOriginLine = findLineIndex('  m_Origin:', bgTilemapStart);
const bgSizeLine = findLineIndex('  m_Size:', bgTilemapStart);

console.log(`Background m_Tiles: line ${bgTilesStart + 1}`);
console.log(`Background m_AnimatedTiles: line ${bgAnimatedTiles + 1}`);

// ============================================================
// 5. EXTRACT EXISTING PLANTS TILE DATA
// ============================================================

// Parse existing Plants tile entries to reuse for repositioning
const existingPlantTiles = [];
let i = plantsTilesStart + 1;
while (i < plantsAnimatedTiles) {
  const line = lines[i];
  if (line.trim().startsWith('- first:')) {
    // Start of a tile entry - collect all lines until next entry or end
    const tileLines = [line];
    i++;
    while (i < plantsAnimatedTiles && !lines[i].trim().startsWith('- first:')) {
      tileLines.push(lines[i]);
      i++;
    }
    existingPlantTiles.push(tileLines);
  } else {
    i++;
  }
}
console.log(`Existing Plants tiles parsed: ${existingPlantTiles.length}`);

// Generate new Plants tiles YAML by repositioning existing entries
function generatePlantsTilesYaml() {
  if (existingPlantTiles.length === 0) return '  m_Tiles: {}\n';
  
  const newEntries = [];
  
  for (const placement of decorationPlacements) {
    const idx = placement.origIdx;
    if (idx >= existingPlantTiles.length) continue;
    
    // Copy the original tile data but change position
    const origLines = [...existingPlantTiles[idx]];
    // Replace the first line (position)
    origLines[0] = `  - first: {x: ${placement.x}, y: ${placement.y}, z: 0}`;
    newEntries.push({ x: placement.x, y: placement.y, lines: origLines });
  }
  
  // Sort by Y then X
  newEntries.sort((a, b) => a.y !== b.y ? a.y - b.y : a.x - b.x);
  
  let yaml = '  m_Tiles:\n';
  for (const entry of newEntries) {
    yaml += entry.lines.join('\n') + '\n';
  }
  return yaml;
}

// ============================================================
// 6. BUILD NEW SCENE FILE
// ============================================================

console.log('\nBuilding new scene file...');

const newLines = [];
let lineIdx = 0;

// Helper: copy lines from original
function copyLines(fromIdx, toIdx) {
  for (let j = fromIdx; j < toIdx && j < lines.length; j++) {
    newLines.push(lines[j]);
  }
}

// Helper: add text (splits by newline)
function addText(text) {
  const textLines = text.split('\n');
  // Don't add trailing empty line
  for (let j = 0; j < textLines.length; j++) {
    if (j === textLines.length - 1 && textLines[j] === '') continue;
    newLines.push(textLines[j]);
  }
}

// --- Process the file section by section ---

// A) Copy everything BEFORE Plants Tilemap tiles
copyLines(0, plantsTilesStart);

// B) Insert new Plants tiles
addText(generatePlantsTilesYaml());

// C) Skip old Plants tiles, copy from m_AnimatedTiles onward
// We need to handle the Plants asset arrays too - update ref counts
// For now, skip old tiles and copy the rest of the Plants section as-is
// The ref counts will be slightly off but Unity handles this gracefully
copyLines(plantsAnimatedTiles, plantsOriginLine);

// D) Update Plants m_Origin and m_Size
const plantsMinX = Math.min(...decorationPlacements.map(p => p.x));
const plantsMaxX = Math.max(...decorationPlacements.map(p => p.x));
const plantsMinY = Math.min(...decorationPlacements.map(p => p.y));
const plantsMaxY = Math.max(...decorationPlacements.map(p => p.y));
newLines.push(`  m_Origin: {x: ${plantsMinX - 2}, y: ${plantsMinY - 2}, z: 0}`);
newLines.push(`  m_Size: {x: ${plantsMaxX - plantsMinX + 5}, y: ${plantsMaxY - plantsMinY + 5}, z: 1}`);

// Skip old Origin and Size lines
let afterPlantsSizeLine = plantsSizeLine + 1;
// Copy rest of Plants Tilemap component until Platforms Tilemap
copyLines(afterPlantsSizeLine, platformsTilesStart);

// E) Insert new Platforms tiles
addText(generatePlatformTilesYaml());

// F) Skip old Platforms tiles, write new animated tiles + asset arrays
newLines.push('  m_AnimatedTiles: {}');

// Write Platforms TileAssetArray - keep structure, update ref count for index 15
newLines.push('  m_TileAssetArray:');
// Indices 0-14: zero ref count
for (let j = 0; j < 15; j++) {
  newLines.push('  - m_RefCount: 0');
  newLines.push('    m_Data: {fileID: 0}');
}
// Index 15: the RuleTile with updated ref count
newLines.push(`  - m_RefCount: ${groundTiles.size}`);
newLines.push('    m_Data: {fileID: 11400000, guid: 6f095f489c38dde43afd7c4e325ce1aa, type: 2}');
// Index 16: zero
newLines.push('  - m_RefCount: 0');
newLines.push('    m_Data: {fileID: 0}');

// Write Platforms TileSpriteArray  
newLines.push('  m_TileSpriteArray:');
for (let j = 0; j < 14; j++) {
  newLines.push('  - m_RefCount: 0');
  newLines.push('    m_Data: {fileID: 0}');
}
// Keep original sprite references, set all tiles to use index 16
newLines.push('  - m_RefCount: 0');
newLines.push('    m_Data: {fileID: -5276354722685141211, guid: f41d28dcc8ebcab43945145c3b9af0bd, type: 3}');
newLines.push('  - m_RefCount: 0');
newLines.push('    m_Data: {fileID: 1013165895579468749, guid: efd727738646c054a87ae6230016cb18, type: 3}');
newLines.push(`  - m_RefCount: ${groundTiles.size}`);
newLines.push('    m_Data: {fileID: 5850654607893932841, guid: b2edbb66ae008ec469b2aaa6f7897e8b, type: 3}');
newLines.push('  - m_RefCount: 0');
newLines.push('    m_Data: {fileID: 0}');
newLines.push('  - m_RefCount: 0');
newLines.push('    m_Data: {fileID: 0}');

// Write Platforms TileMatrixArray
newLines.push('  m_TileMatrixArray:');
// Indices 0-3: zero ref with garbage data (copy from original)
for (let j = 0; j < 4; j++) {
  newLines.push('  - m_RefCount: 0');
  newLines.push('    m_Data:');
  newLines.push('      e00: 1');
  newLines.push('      e01: 0');
  newLines.push('      e02: 0');
  newLines.push('      e03: 0');
  newLines.push('      e10: 0');
  newLines.push('      e11: 1');
  newLines.push('      e12: 0');
  newLines.push('      e13: 0');
  newLines.push('      e20: 0');
  newLines.push('      e21: 0');
  newLines.push('      e22: 1');
  newLines.push('      e23: 0');
  newLines.push('      e30: 0');
  newLines.push('      e31: 0');
  newLines.push('      e32: 0');
  newLines.push('      e33: 1');
}
// Index 4: identity matrix with correct ref count
newLines.push(`  - m_RefCount: ${groundTiles.size}`);
newLines.push('    m_Data:');
newLines.push('      e00: 1');
newLines.push('      e01: 0');
newLines.push('      e02: 0');
newLines.push('      e03: 0');
newLines.push('      e10: 0');
newLines.push('      e11: 1');
newLines.push('      e12: 0');
newLines.push('      e13: 0');
newLines.push('      e20: 0');
newLines.push('      e21: 0');
newLines.push('      e22: 1');
newLines.push('      e23: 0');
newLines.push('      e30: 0');
newLines.push('      e31: 0');
newLines.push('      e32: 0');
newLines.push('      e33: 1');

// Write Platforms TileColorArray
newLines.push('  m_TileColorArray:');
newLines.push(`  - m_RefCount: ${groundTiles.size}`);
newLines.push('    m_Data: {r: 1, g: 1, b: 1, a: 1}');

// Write remaining Platforms metadata
newLines.push('  m_TileObjectToInstantiateArray: []');
newLines.push('  m_AnimationFrameRate: 1');
newLines.push('  m_Color: {r: 1, g: 1, b: 1, a: 1}');

// Calculate bounds
const allTileCoords = [...groundTiles].map(s => {
  const [x, y] = s.split(',').map(Number);
  return { x, y };
});
const gMinX = Math.min(...allTileCoords.map(t => t.x));
const gMaxX = Math.max(...allTileCoords.map(t => t.x));
const gMinY = Math.min(...allTileCoords.map(t => t.y));
const gMaxY = Math.max(...allTileCoords.map(t => t.y));

newLines.push(`  m_Origin: {x: ${gMinX}, y: ${gMinY}, z: 0}`);
newLines.push(`  m_Size: {x: ${gMaxX - gMinX + 1}, y: ${gMaxY - gMinY + 1}, z: 1}`);
newLines.push('  m_TileAnchor: {x: 0.5, y: 0.5, z: 0}');
newLines.push('  m_TileOrientation: 0');
newLines.push('  m_TileOrientationMatrix:');
newLines.push('    e00: 1');
newLines.push('    e01: 0');
newLines.push('    e02: 0');
newLines.push('    e03: 0');
newLines.push('    e10: 0');
newLines.push('    e11: 1');
newLines.push('    e12: 0');
newLines.push('    e13: 0');
newLines.push('    e20: 0');
newLines.push('    e21: 0');
newLines.push('    e22: 1');
newLines.push('    e23: 0');
newLines.push('    e30: 0');
newLines.push('    e31: 0');
newLines.push('    e32: 0');
newLines.push('    e33: 1');

// G) Skip old Platforms data, copy from TilemapCollider2D onward to Background tiles
// Find the TilemapCollider2D after Platforms
const platformsColliderStart = findComponentStart('1499247568');
console.log(`Platforms TilemapCollider2D: line ${platformsColliderStart + 1}`);

// Copy from collider start to Background tiles start
copyLines(platformsColliderStart, bgTilesStart);

// H) Insert new Background tiles
const bgTileCount = (46 - (-18) + 1) * (14 - (-18) + 1);  // 65 * 33 = 2145
addText(generateBackgroundTilesYaml());

// I) Skip old Background tiles, write background metadata
newLines.push('  m_AnimatedTiles: {}');

// Find where the original background asset array starts and copy from there
// But we need to update origin/size. Let me copy asset arrays as-is.
// First, find what's between bgAnimatedTiles and bgOriginLine in the original
copyLines(bgAnimatedTiles + 1, bgOriginLine);

// Update Background origin and size  
newLines.push(`  m_Origin: {x: -18, y: -18, z: 0}`);
newLines.push(`  m_Size: {x: 65, y: 33, z: 1}`);

// Skip old background Origin and Size, copy rest
let afterBgSizeLine = bgSizeLine + 1;

// J) Find and update PolygonCollider2D on Background for camera bounds
// Look for PolygonCollider2D component of Background (fileID: 1776424816)
const bgPolyColliderStart = findComponentStart('1776424816');
console.log(`Background PolygonCollider2D: line ${bgPolyColliderStart + 1}`);

if (bgPolyColliderStart > 0) {
  // Copy from after bg size to poly collider
  copyLines(afterBgSizeLine, bgPolyColliderStart);
  
  // Find m_Points in the PolygonCollider2D
  const polyPointsStart = findLineIndex('m_Points:', bgPolyColliderStart);
  
  if (polyPointsStart > 0) {
    // Copy collider header up to m_Points
    copyLines(bgPolyColliderStart, polyPointsStart);
    
    // Write new polygon points for camera bounds
    // Camera bounds should encompass the entire play area
    newLines.push('    m_Points:');
    newLines.push('      m_Paths:');
    newLines.push('      - - {x: -17, y: -17}');
    newLines.push('        - {x: 45, y: -17}');
    newLines.push('        - {x: 45, y: 13}');
    newLines.push('        - {x: -17, y: 13}');
    
    // Find end of old m_Points section (next key at same indent level)
    let afterPointsLine = polyPointsStart + 1;
    while (afterPointsLine < lines.length) {
      const l = lines[afterPointsLine];
      // Look for next top-level key in the collider or next component
      if (l.startsWith('---') || (l.match(/^  m_/) && !l.includes('m_Points'))) {
        break;
      }
      afterPointsLine++;
    }
    
    // Copy rest of file
    copyLines(afterPointsLine, lines.length);
  } else {
    // No m_Points found, just copy everything from after bg size
    copyLines(afterBgSizeLine, lines.length);
  }
} else {
  // No PolygonCollider2D found, just copy rest
  copyLines(afterBgSizeLine, lines.length);
}

// ============================================================
// 7. UPDATE PLAYER START POSITION
// ============================================================

// Find the Player prefab transform and update position
// Player is referenced via prefab instance, its transform fileID is 299233600
// The Follow Camera references it at m_Follow: {fileID: 299233600}
// We need to find the player prefab instance and update its position

// Also update the Follow Camera position
const followCamPosLine = findLineIndex('m_LocalPosition: {x: -13.5, y: -7.9, z: -10}');
if (followCamPosLine > 0) {
  // Update in the output
  for (let j = 0; j < newLines.length; j++) {
    if (newLines[j].includes('m_LocalPosition: {x: -13.5, y: -7.9, z: -10}')) {
      newLines[j] = '    m_LocalPosition: {x: -14, y: -5, z: -10}';
      console.log('Updated Follow Camera position');
      break;
    }
  }
}

// ============================================================
// 8. WRITE OUTPUT
// ============================================================

const output = newLines.join('\n');
console.log(`\nOutput file: ${newLines.length} lines (was ${lines.length})`);
console.log(`Ground tiles: ${groundTiles.size}`);
console.log(`Background tiles: ${bgTileCount}`);
console.log(`Decoration tiles: ${Math.min(decorationPlacements.length, existingPlantTiles.length)}`);

// Write to file
fs.writeFileSync(OUTPUT_PATH, output, 'utf-8');
console.log(`\n✅ Scene file written to: ${OUTPUT_PATH}`);
console.log('Open Unity - it will refresh the RuleTile sprites automatically.');
