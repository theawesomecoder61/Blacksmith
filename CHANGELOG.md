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
| Geometry            | 🗸                         | 🗸                         | X     |
| Normals             | 🗸                         | 🗸                         | X     |
| UVs                 | 🗸                        | 🗸                         | X     |
| UV sets             | X                         | X                         | X     |
| Skeleton            | X                         | X                         | X     |

### Other
|    Type    | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep |
|------------|---------------------------|---------------------------|-------|
| Soundpacks | 🗸                         | 🗸                        | 🗸     |
| Soundbanks | 🗸                         | 🗸                        | 🗸     |

## Changelog
### Version 1.6 [CURRENT VERSION] (4/20/2019)
- Added
  - Texture Set support, data will display in the Text Viewer
  - "Multifile Entry" support, any entry that contains additional files (Build Table, LOD Selector, to name a couple)
    - **found by right-clicking an entry or subentry and selecting `Multifile Entry > Extract Contained Files`, which reveals the Multifile Entry Exporter**
  - `Tools > Show File in the Viewers` and `Tools > Export 3D Viewer Contents`; both features are self-explanatory
  - number of results displays in the Find window
  - `Filter By` in the Find window (easily locate 3D models and textures)
  - automatic normal map fixing feature (flips blue channel; credit to @cire992), can be turned off in the Settings
  - 500+ new Resource Identifiers
  - if the entry is under 2 MB (compressed), all located Resource Identifiers will be displayed; otherwise, only the first Resource Identifier will be displayed
  -- **note: extraction may take slightly longer as a result**
- Updated
  - you can now press Escape to close any popup/dialog
  - renamed "ResourceType" to "ResourceIdentifier"
  - replaced the teapot with a cube until I feel like fixing the OBJ importer
  - rewrote the logic of the texture map importers
  - Blacksmith now employs a custom version of texconv (inverty was replaced with invertz)
  - DDS generation logic
  - cleaned up code
- Fixed
  - a temporary file path will be created and set if the user did not set one
  - 3D models with skeletons and skeleton-related data are handled better with both AC games
  - OBJ exporter; now you can import models into your preferred software without errors, though UVs do not always come out properly
  - the Resource Identifiers Viewer would not open if the user right-clicked on a subentry/Resource Identifier
  - the Image Viewer would show its previous image by changing to zoom level if a different (3D or text) asset was loaded
  - a crash caused by opening a GlobalMetaFile from Steep
  - various issues

### Version 1.5.1 (4/1/2019)
- Added
  - an option to exclude normals of 3D models in `Save As...` dialog
  - a setting to suppress success, warning, and failure popups
  - `Controls Help` in the 3D Viewer - you can view the controls for the 3D Viewer
  - loading and saving Settings to .ini files
  - ini-parser, a libray for reading and writing .ini files
- Updated
  - subentries/Resource Types will not show multiple times
  - organized messages/popups throughout
  - better Build Table support
- Fixed
  - ***texture conversion works again*** (texconv works again)
  - DDS textures will not be converted from DDS
  - the background color will be selected in the Color dialog when you open Settings
  - your session will not be lost if you close the Settings, unless you changed the path of a game or the temporary file path
  - a message will inform you that a Forge has not been selected in the Find window if you attempt to perform a search
  - a crash caused by loading a Steep 3D model (support will come in Version 1.6)
  - OBJ exporter again

### Version 1.5 (preview) (3/31/2019)
- Added
  - initial Odyssey model support
  - the checkbox list next to the 3D Viewer - this allows you to toggle individuals meshes
  - added `Show In Explorer` context menu item
  - added more features to Find dialog: double-clicking reveals the entry in Blacksmith, "Forge to search in"
  - "Forge to search in" in the Find window
    - this allows you to specify which open .forge file to search, instead of the first open .forge file
  - `Save As...` window to simplify saving as other formats
- Updated
  - better Origins 3D model support
  - 3D Viewer displays models 5x larger
  - output from 3D model in the Text Viewer
  - the split containers update while dragging the splitter
  - various UI and behavior improvements
  - DAE, SMD, STL export removed temporarily
- Fixed
  - OBJ exporter
- **Texture conversion will sit out on this version. texconv is not being friendly.** Expect this fixed in the next version.

### Version 1.4 (3/8/2019)
- Added
  - *initial Origins 3D model support!*
  > Supported features: geometry, normals, texture coordinates (UVs)
  - added ability to export a 3D model to DAE and OBJ
  - `Tools > Preview an Origins Model`
  - adjust the size of points from the 3D Viewer in Settings
  - a `Copy Name to Clipboard` item to the context menu
  - a `Datafile > Show Resource Identifiers Viewer` item
  > Resource Identifiers Viewer displays all located resource identifiers and offsets in that entry
- Updated
  - the ResourceType enum and its extension class
  - faster and better Resource Type detection
  - 3D Viewer camera moves slower
  - 3D Viewer camera only moves if the cursor is on the 3D Viewer (all input is directed to it)
  - 3D Viewer now renders the model with its normals (colorful)
  - other 3D Viewer improvements
- Fixed
  - file sizes over 2 GB would not show in the status bar
  - improved Steep texture support
  - the inability to extract image data from a TopMip/Mip if there is only one TopMip/Mip
  - the tree view would lose focus and the selected node would be unselected
  - the useless `Decompress Localization Data` menu item was removed from sight
  - Find feature works
  - various UI improvements

### Version 1.3.1 (2/9/2019)
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

### Version 1.3 (2/18/2019)
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
