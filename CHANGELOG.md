## Current Support
### Textures
| Texture Type | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep |
|--------------|---------------------------|---------------------------|-------|
| Diffuse      | 🗸                        | 🗸                        | 🗸     |
| Normal       | 🗸                        | 🗸                        | 🗸     |
| Mask         | 🗸                        | 🗸                        | 🗸     |
| Specular     | 🗸                        | 🗸                        | 🗸     |
| UI/HUD       | X                        | X                        | X     |

### 3D Models
|  3D Model Features  | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep |
|---------------------|---------------------------|---------------------------|-------|
| Geometry            | X                         | X                         | X     |
| Normals             | X                         | X                         | X     |
| UV sets             | X                         | X                         | X     |
| Skeleton & Skinning | X                         | X                         | X     |

### Other
|    Type    | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep |
|------------|---------------------------|---------------------------|-------|
| Soundpacks | 🗸                         | 🗸                        | 🗸     |
| Soundbanks | 🗸                         | 🗸                        | 🗸     |

## Changelog
### Version 1.3 [POSSIBLE FEATURES] (2/XX/2019)
*This is a list of possible features. The list is incomplete.*
- Added
  - **Full support for textures** for all games
    > Includes UI/HUD textures
- Fixed
  - Find menu and window work properly
- Updated
  - Image Viewer
    > The background is now a grid, zoom works on all images
  - Image View background color removed from the Settings
### Version 1.2 [CURRENT VERSION] (2/3/2019)
- Added
  - **Texture conversion support for all games**
  - *Partial* Steep texture extraction and viewing support
  - Blacksmith now has an icon! (credit to Krisada on the Noun Project)
  - **Full support for .pck (soundpacks) and .bnk (soundbanks)**
    > The soundpack contents will appear in the Soundpack Browser, where you can extract sounds
  - Find menu and window (does not work yet)
  - Texture context menu
  - Zoom Level to the Image Viewer
    > You can zoom on an image now
  - Localization-related enums to the ResourceType class
  - bnkextr, revorb, and ww2ogg
  - NAudio and NAudio.Vorbis
- Fixed
  - Texture extraction/conversion occurs much quicker
    > The Image Viewer becomes active with the texture already loaded
  - Converted textures will not be converted again if the user clicks on the entry/subentry again
- Updated
  - x86 versions of lzo and Oodle are no longer included with the releases
  - Entries already loaded will appear individually (especially with large .forge files)
  - Assembly information
  - The progress bar animates while using the Tools > Decompress File
  - Decompress File now asks for a save location
  - The Path status label will no longer show paths within .forge files

### Version 1.1 (1/23/2019)
- Added
  - Initial Steep support! (do not expect full texture support until Version 1.2)
  - Support for ***viewing of textures*** in Odyssey
  - *Partial* support for ***viewing of textures*** in Origins
  - Support for `.log` files (viewable in the Text Viewer)
  - Functionality to the `Create Filelist` and `Extract All` context menus
    - Go in the Settings to change the output of the filelist with tabs or commas
  - `Tools > Decompress File` (where you can decompress an extracted file)
  - The ability to change the background color of the Image Viewer (in the Settings)
- Fixed
  - Updated the version of texconv
  - The `More > Source Code` menu takes you to this repo (instead of crashing)
  - Made the context menu foolproof
- Other
  - More preparation for the 3D Viewer (no word on its release)
  - More undocumented changes

### Version 1.0 (1/19/2019)
- Initial version
- Known issues:
  - 3D Viewer does not show anything, yet
  - Image Viewer does not support .dds, yet (it supports .png)
