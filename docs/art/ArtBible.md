# Art Bible
**SHGame - Unity 2D PC Single-Player Game**

*Version: 0.1 | Date: TBD*

---

## 🎨 Visual Identity

### Art Direction
- **Style**: *[e.g., Pixel Art, Hand-drawn, Minimalist, etc.]*
- **Mood**: *[e.g., Whimsical, Dark, Adventurous, Peaceful]*
- **Inspiration**: *[Reference games, movies, or art styles]*

### Visual Pillars
1. **[Pillar 1]**: *Visual goal or theme*
2. **[Pillar 2]**: *Visual goal or theme*
3. **[Pillar 3]**: *Visual goal or theme*

---

## 🎨 Color Palette

### Primary Colors
- **Primary**: `#[HEX CODE]` - *Usage description*
- **Secondary**: `#[HEX CODE]` - *Usage description*
- **Accent**: `#[HEX CODE]` - *Usage description*

### UI Colors
- **Background**: `#[HEX CODE]`
- **Text Primary**: `#[HEX CODE]`
- **Text Secondary**: `#[HEX CODE]`
- **Button Default**: `#[HEX CODE]`
- **Button Hover**: `#[HEX CODE]`
- **Button Active**: `#[HEX CODE]`

### Gameplay Colors
- **Player**: `#[HEX CODE]`
- **Enemies**: `#[HEX CODE]`
- **Collectibles**: `#[HEX CODE]`
- **Environment**: `#[HEX CODE]`
- **Interactive Objects**: `#[HEX CODE]`

---

## 📐 Technical Specifications

### Asset Resolution
- **Sprite Resolution**: *[e.g., 32x32, 64x64, 128x128 pixels]*
- **UI Resolution**: *[e.g., 1920x1080 design canvas]*
- **Texture Size**: *[Maximum texture dimensions]*
- **Pixels Per Unit**: *[Unity PPU setting, e.g., 100]*

### File Formats
- **Sprites**: PNG (with transparency)
- **UI Elements**: PNG (with transparency)
- **Backgrounds**: PNG or JPG (based on transparency needs)
- **Source Files**: *[e.g., .psd, .aseprite, .sketch]*

### Naming Conventions
```
Characters: character_[name]_[state].png
  Example: character_player_idle.png

Environment: env_[type]_[variant].png
  Example: env_tree_01.png

UI: ui_[element]_[state].png
  Example: ui_button_normal.png

Effects: fx_[type]_[frame].png
  Example: fx_explosion_01.png
```

---

## 👾 Character Art

### Player Character
- **Design Description**: *[Appearance, personality through design]*
- **Color Scheme**: *[Specific colors used]*
- **Animation States**: 
  - Idle
  - Walk/Run
  - Jump (if applicable)
  - *[Other states as needed]*

### Character Animation Specifications
- **Frame Rate**: *[e.g., 12 FPS, 24 FPS]*
- **Frame Count**: 
  - Idle: *[X frames]*
  - Walk: *[X frames]*
  - Jump: *[X frames]*
- **Loop Type**: *[Which animations loop]*

---

## 🌍 Environment Art

### World Themes
- **Theme 1**: *[Description and visual elements]*
- **Theme 2**: *[Description and visual elements]*
- **Theme 3**: *[Description and visual elements]*

### Environment Elements
| Element Type | Style Notes | Color Guidelines | Size Guidelines |
|--------------|-------------|------------------|-----------------|
| Ground/Platforms | *Style description* | *Color range* | *Dimensions* |
| Background | *Style description* | *Color range* | *Dimensions* |
| Decorative | *Style description* | *Color range* | *Dimensions* |
| Interactive | *Distinct styling* | *Highlight colors* | *Dimensions* |

### Parallax Layers (if applicable)
1. **Background**: *[Farthest layer, slowest scroll]*
2. **Midground**: *[Middle layer, medium scroll]*
3. **Foreground**: *[Closest layer, fastest scroll]*
4. **Gameplay Layer**: *[Where player and interactive elements exist]*

---

## 🔘 UI/UX Design

### UI Style
- **Theme**: *[Modern, Retro, Fantasy, etc.]*
- **Button Style**: *[Flat, Raised, Outlined, etc.]*
- **Typography**: *[Font choices and hierarchy]*
- **Iconography**: *[Icon style and guidelines]*

### Menu Layouts
- **Main Menu**: *[Layout description and key elements]*
- **Pause Menu**: *[Layout description and key elements]*
- **Settings Menu**: *[Layout description and key elements]*
- **Game HUD**: *[Layout description and key elements]*

### UI Animation Guidelines
- **Transitions**: *[Fade, Slide, Scale, etc.]*
- **Duration**: *[Typical animation length]*
- **Easing**: *[Animation curves and feel]*
- **Feedback**: *[How UI responds to user input]*

---

## ⚡ Effects & Polish

### Visual Effects
- **Particle Effects**: *[Style and usage guidelines]*
- **Screen Effects**: *[Screen shake, flashes, etc.]*
- **Transitions**: *[Scene transitions, wipes, etc.]*

### Polish Elements
- **Juice**: *[Small details that add personality]*
- **Feedback**: *[Visual responses to player actions]*
- **Ambient Details**: *[Background animations, etc.]*

---

## 📱 Platform Considerations

### PC Specific
- **Mouse Cursor**: *[Custom cursor design if needed]*
- **Window Icon**: *[Application icon specifications]*
- **Resolution Scaling**: *[How art scales across resolutions]*

### Accessibility
- **Color Blindness**: *[Ensure contrast and alternative indicators]*
- **Text Readability**: *[Font size and contrast guidelines]*
- **Visual Clarity**: *[Clear distinction between interactive elements]*

---

## 🔧 Implementation Guidelines

### Unity Setup
- **Sprite Import Settings**: 
  - Sprite Mode: Single/Multiple
  - Pixels Per Unit: *[Value]*
  - Filter Mode: Point/Bilinear
  - Compression: None/High Quality

### Animation Setup
- **Animator Controllers**: *[Structure and organization]*
- **Animation Clips**: *[Naming and organization]*
- **Sprite Sheets**: *[Layout and optimization]*

### Optimization
- **Texture Atlasing**: *[Sprite packing strategy]*
- **Draw Call Reduction**: *[Batching guidelines]*
- **Memory Usage**: *[Texture size optimization]*

---

## 📋 Asset Checklist

### Before Adding New Art Assets
- [ ] Follows established color palette
- [ ] Matches art style consistency
- [ ] Proper naming convention used
- [ ] Appropriate resolution/size
- [ ] Optimized file size
- [ ] Source file archived (if applicable)

### Quality Assurance
- [ ] Art displays correctly in Unity
- [ ] No visual artifacts or compression issues
- [ ] Consistent lighting/shading
- [ ] Proper transparency handling
- [ ] Performance impact assessed

---

## 🤖 AI Validation Points

### Automated Checks
- [ ] Asset naming follows conventions
- [ ] File sizes within limits
- [ ] Required sprite settings applied
- [ ] No missing texture references

### Visual Consistency Review
- [ ] New assets match established palette
- [ ] Art style consistency maintained
- [ ] UI elements follow design system
- [ ] Character animations are smooth

### Integration Testing
- [ ] Assets display correctly in scenes
- [ ] UI scales properly across resolutions
- [ ] Animations play without errors
- [ ] Performance targets maintained

---

## 📚 Reference Gallery

### Style References
*[Include or link to reference images, mood boards, or inspiration sources]*

### Asset Examples
*[Examples of completed assets that represent the art direction]*

---

## 🗓️ Art Production Pipeline

### Workflow
1. **Concept**: Sketch and design new assets
2. **Creation**: Produce final art assets
3. **Review**: Check against art bible guidelines
4. **Implementation**: Import and setup in Unity
5. **Testing**: Verify in-game appearance
6. **Optimization**: Adjust for performance if needed

### Tools & Software
- **Primary**: *[Main art creation software]*
- **Secondary**: *[Additional tools used]*
- **Unity Version**: Unity 6000+

---

*Last Updated: [Date] | Art Review: [Date]*