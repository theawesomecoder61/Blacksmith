## Current Support
### Textures
| Texture Type | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep |
|--------------|---------------------------|---------------------------|-------|
| Diffuse      | 🗸                        | 🗸                         | 🗸     |
| Normal       | 🗸                        | 🗸                         | 🗸     |
| Mask         | 🗸                        | 🗸                         | 🗸     |
| Specular     | 🗸                        | 🗸                         | 🗸     |
| UI/HUD       | 🗸                        | 🗸                         | 🗸     |

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
### Version 1.3.1 [FUTURE VERSION] (2/19/2019)
- Fixed
  - extraction and loading issues regarding textures with TopMips/Mips
  - a crash that would arise after the user selects "No" from the "Warning" message when attempting to load a .forge with over 20,000 entries
  - that same message showed if a .forge has over 10,000 entries (though the message says "20,000") - this has been adjusted to 20,000
  - decompression and extraction issues
  - removed the `Convert` context menu item
  - fixed context menu items that were not working or caused crashes
  - the Image Viewer's scrollbars show when the image should be scrollable (that is, when the image is not entirely visible)
  - a message shows if a texture cannot be converted (instead of crashing)
  - the Tag property of the EntryTreeNode class has been superseded by the Path property
  - the 3D Viewer will not respond to camera movements unless Blacksmith is forefront and focused

### Version 1.3 [CURRENT VERSION] (2/18/2019)
- Added
  - the 3D Viewer is all set to go
  - extensions for decompressed data for each game (.acod, .acor, .stp)
- Updated
  - you are no longer able to set the background color of the Image Viewer because the background is now a grid
  - images displayed in the Image Viewer will be centered
  - images are now oriented correctly (rotated 180 degrees and flipped along the X-axis)
  - I rewrote a chunk of the Form1 class
  - **the loading of entries occurs much quicker (insanely fast!)**
  - abandoned SlimDX in favor of OpenTK
  - the 3D Viewer background color in the Settings window is enabled
  - raw data is no logner written to a file
  - deleted the ThreeD folder
  - created a new 3D-related folder ("Three")
  - Settings window more organized
  - Help is now implemented
  - the tree view and Soundpack Browser are temporarily disabled while entries are loading (to prevent the user from overworking the program)
- Fixed
  - all entries from each .forge are loaded
  - zoom works with all images
  - any issue that arises when you browse multiple .forge files at once

### Version 1.2 (2/3/2019)
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
