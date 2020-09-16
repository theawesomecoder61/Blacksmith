## Current Support
### Textures
| Texture Type | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep | Ghost Recon Breakpoint |
|--------------|---------------------------|---------------------------|-------|------------------------|
| Diffuse      | ✔️                        | ✔️                       | ✔️    | future                 |
| Normal       | ✔️                        | ✔️                       | ✔️    | future                 |
| Mask         | ✔️                        | ✔️                       | ✔️    | future                 |
| Specular     | ✔️                        | ✔️                       | ✔️    | future                 |
| UI/HUD       | ✔️                        | ✔️                       | ✔️    | future                 |

### 3D Models
|  3D Model Features  | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep | Ghost Recon Breakpoint |
|---------------------|---------------------------|---------------------------|-------|------------------------|
| Geometry            | ✔️ (nearly 100%)          | ✔️                       | ✔️  | future                  |
| Normals             | ✔️                        | ✔️                       | ✔️ | future                   |
| UVs                 | ✔️                        | ✔️                       | ✔️ | future                   |

### Other
|    Type    | Assassin's Creed: Odyssey | Assassin's Creed: Origins | Steep | Ghost Recon Breakpoint |
|------------|---------------------------|---------------------------|-------|------------------------|
| GlobalMetaFile | ✔️                   | ✔️                        | ✔️    | future                 |
| Materials  | ✔️                       | ✔️                        | ✔️    | future                 |
| Soundbanks | ✔️                       | ✔️                        | ✔️    | future                 |
| Soundpacks | ✔️                       | ✔️                        | ✔️    | future                 |
| Texture Sets | ✔️                     | ✔️                        | ✔️    | future                 |

## Changelog
## 1.9.4 Public Beta
- 9/16/2020
- updated
  - (3D Viewer) models and grid are proper scaling in meters
  - (3D Viewer) the model will not longer be adjusted in any form after reading the data and manipulating it once
  - (3D Viewer) vastly improved camera and controls (now a modified arcball camera)
  - (3D Viewer) new control: Z - Move Down, and flipped actions of the Q and E keys
  - (3D Viewer) replaced `Show Grid` with `Model Size`, which reports the size of the current model
  - (Save As) the format dropdowns will now default to the user's default model and texture settings
  - (Save As) removed the elusive "Combine Meshes into a Single File" completely
  - no more extra scaling, even to exported models
  - reduced overall camera speed
  - removed the UV Viewer, too slow and prone to failing
  - (Donate Window) now uses a webview to display the text (IE 11)
  - removed AxisGizmo
  - removed "Use 1.7 and Older Camera" setting
  - removed glTF support (it never worked properly)
   - if you had the "Default Model Format" setting set to glTF, it will be switched to 3ds
  - UI changes & strings
  - removed some unneeded logging
- fixed
  - Odyssey model importing logic (help from hypermorphic/jjj)
    - Steep model and texture problems were also fixed
  - proper Resource Identifier names
  - (Save As) FBX 7.5 Binary now saves in Binary
  - (Welcome) "AC: Origins - Set" and "Steep - Set" messages now show on each game's respective button
  - (Welcome) patched a "not enough memory resources" issue
  - (Welcome) additional fixes & changes
  - File ID on the bottom toolbar shows up more reliably when loading a file from *File > Open File*
  - "None" will no longer show under a forge entry
  - texconv's window will no longer show

### Version 1.9.3
- [CURRENT VERSION] (4/2/2020)
- added
  - a meter (next to the Save As button) that ticks down the number of exports left in a session
    - the value resets each time you open Blacksmith
    ![meter](http://t-poses.com/static/img/1.9.3-meter.png)
- fixed
  - certain head models from Origins could not be loaded

### Version 1.9.2
- (10/3/2019)
- added
  - Blacksmith will flash orange in the taskbar upon completing a successful task (regardless of the "Hide Popups" setting)
- updated
  - the Save As window will close after saving
  - the number of vertices, faces, and meshes are no longer updated every 1/60 sec. (16.6 ms.), instead are updated only when the model changes
  - FlashWindow and Message classes now belong to Blacksmith.Helpers
- fixed
  - stretched-out 3D Viewer
  - a result found in the list, by double-clicking (or right-clicking > Show in List), could lose focus in the list
    - the fix is that the result will appear green in Blacksmith
  - Find window UI fixes
    - the splitter covered "Filter By"
    - the column headers covered the column text
    
### Version 1.9.1
- (9/29/2019)
- updated
  - updated Oodle's binary to the latest version
- fixed
  - UVs were incorrect on certain meshes with external UV data - affected Odyssey and Origins
  - Blacksmith threw a fit if the user loaded an ini file from an older version on the Welcome dialog - issue with the "Lang" property not found
  - improper scaling if the user set the DPI scaling (in Display settings) greater than 100%

### Version 1.9
- (8/27/2019)
- added
  - Steep ⛷ 🏂
    - partial 3D model support
    - Material support
    - Texture Set support
  - GlobalMetaFile support for all three games (Steep's GlobalMetaFiles do not have unhashed names, but instead will use a list of known hashes)
  - German translations (credit: xBaebsae)
  - Portugese (Portugal) translations (credit: Messias)
  - Table Viewer (used by Texture Sets and Meshes now)
  - "Tools > Convert > WEM --> OGG"
  - "Compiled Texture Map", "Mip", "Skeleton", and "Material Template" to the Forge Filter (Settings) and Filter By (Find)
  - Material reports are generated when a Material is selected from the entries list (hierarchy)
  - after loading a model, look in the Table Viewer to see if Materials referenced by the model were found (for all games)
  - icons for each game
  - shortcut to "Tools > Export 3D Viewer Contents", Ctrl+Shift+E
  - new Settings to the Behavior section: Default Model Format, Default Normals Mode, Default Texture Format
    - these preload the values for the formats and Normals Mode in the Save As window, so that it saves you time when exporting files
  - Blacksmith.IO.Reader, an extension to BinaryReader
- updated
  - **"Forge Filter" is now called "Resource Types to Display"** to avoid further confusion
  - "Tools > Show File in the Viewers" was moved to "File > Open File..." (shortcut: Ctrl+O)
  - Materials and Texture Sets yield better reports
  - moved "Tools > Decompress File" and "Tools > Decompress Folder" to "Tools > Decompress > ..."
  - "Tools > Decompress > ..." supports LZO (1X, 1C, 2A) compression (which means data from older AC games [III, Black Flag, Unity, Syndicate, etc.] can be decompressed, NOT READ)
  - Lang.Form1 is now Lang.MainWindow, reflected also in MainWindow
  - updated "excess" shaders were removed from this and future releases of Blacskmith
    - donators still have access to all removed shaders in the source code
  - popups that were not translated before are now translated
  - how icons are located
  - text size of the Text Viewer
  - Settings, internally and externally (UI)
  - the Welcome window
  - FastTreeView and EntryTreeNode are now in Blacksmith.Controls
  - ConvertDialog is now SaveAsDialog
  - sped up reading raw data for each game
  - cleaned up Find-related code in MainWindow and Helper
  - removed the usused and hidden "Tools > Decompress Localization Data"
- fixed
  - saving/converting a texture failed
  - user was able to create duplicates of the Save As window
  - Save Query (Settings) did nothing, now it does
  - after auto-texturing worked its magic, it would generate files without a file ID - this has been fixed
  - searching by file ID yielded no results when "<All Loaded Forges>" was selected
  - Find menu item was not translated
  - after closing the Settings, the language would change to a different one
  - the Render Modes were not translated
  - Steep's file IDs were incorrect
  - Steep's forges were not recognized by the Find window
  - all popups were "hidden" or not directly apparent to the user when they appeared
  - the grid would be counted in the total vertices and meshes in the 3D Viewer
  - Resource Types to Display (a.k.a. Forge Filters) will adapt if the newer version has a different amount of resource types

### Version 1.8.1
- (7/18/2019)
- added
  - Show Grid in the 3D Viewer
  - the inner-workings of translation support
  - Russian translation (credit: MediAsylum)
  - I18N-Portable, a library to help with localization
- updated
  - the Save As dialog no longer is a modal (always visible until it is closed)
  - the Settings window with a new Behavior section
  - did more things with the upcoming filelists feature
  - Forge class
  - renamed "Form1" to "MainWindow" and moved it to Blacksmith.Forms
  - the TextureSet reading logic in Odyssey uses ulongs instead of longs
- fixed
  - camera issues (credit: hypermorphic/jjj)
  - model scaling issues
  - after clicking the "Actually, I have..." button (in the Welcome window),  the file chooser dialog would not allow the user to select an .ini file
  - incorrect World resource identifier icon and Terrain Node Data did not have an icon (but it was still located in the files)
  - the Resource Identifier named "CellDataBlock" does not exist, the correct name is "GridCellDataBlock"

### Version 1.8
- (7/9/2019)
- added
  - "Save As" to the 3D Viewer
  - "<All Loaded Forges>" in the "Forge to search in" - guess what it does?
  - the user can now search by a file ID in the Find window (check the "Is File ID?" checkbox)
  - "Auto-Rotate" in the 3D Viewer
  - the Blacksmith icon in the Windows Taskbar will display progress, if a task (extraction, decompression) is ongoing
  - glTF (1.0 binary) support for saving models
  - winsparkle - a framework for checking and automatically installing updates
  - Blacksmith will scan each entry/file loaded from `Tools > Show File in the Viewers` and check if it has Packed Entries
- updated
  - renamed "Multifile Entries" to "Packed Entries"
  - the donate window will not show up upon the first launch
  - the Settings window
    - added "File Types to Show within Forges" (Miscellaneous) allows the user to filter (like in the Find window) which file types he wants to see
    - added "Invert Mouse Y" (found in the 3D Viewer section in the Settings)
    - added "Do Not Unload Forges After Collapsing Them" (Miscellaneous) - this does not freeze the UI; any collapsed and loaded forge entry will turn green
    - added "Image Viewer" box
- updated
  - Settings save upon changing any value
  - Origins 3D model importer - implemented the other known type of mesh structure
  - removed the mouse-locking feature of the 3D Viewer
  - toned down the Welcome window (it was too angry, a user said)
  - the logic of the forge unloading - now the green background indicates that the forge was loaded and all its entries have been created - the background reverts to white if the forge was unloaded (collapsed) and the "Do not unload..." is unchecked
  - the checkbox lists in the 3D Viewer and Settings are more responsive
  - file-writing in the Temporary File Path (file names now include the file ID)
  - loading, saving, and handling the settings (now settings are written to settings.ini in the Temporary File Path upon closing the Settings window, and is read at launch)
  - 3D Viewer
    - added a grid
    - moved Render Mode from Settings (no longer saves in Settings) to the toolbar
    - new key: hold Left Control to make the camera move slow
    - improved rendering and camera mechanics
    - new default camera position and rotation
  - Image Viewer
    - scroll the mouse wheel to zoom (enable/disable in the Settings)
    - click and drag the middle mouse button/mouse wheel to pan
    - zooming is now a lot quicker, for two reasons
      - no more smoothing (pixelated appearance)
      - the original image is stored in memory, instead of being pulled from the file upon each zoom increment
  - Find feature and window
    - added File ID to the results table
    - results in the window will show icons (just as you see in the file list) and the overarching Resource Identifier
    - improved the result of the "Time Taken" in the Find window
    - the entry's file ID will appear in the extracted file to avoid conflicts with duplicate names
    - in the "Filter By" dropdown, "Mesh (3D model)" is now "Mesh"
    - "Show in List" now searches by file ID, instead of name
    - improved overall Find mechanics
  - the Welcome dialog
    - added "Actually, I have a Settings.ini file", which allows the user to load a settings.ini file previously saved from another version of Blacksmith
  - the progress bar on the bottom is more meaningful, and shows proper loading/extraction/decompression progress
  - model information is no logner printed to the Text Viewer (or anywhere)
  - error messages
  - updated UI (3D Viewer, About window, etc.)
  - renamed Helpers to Helper and moved it to Blacksmith.Helpers
  - renamed Cube class to Box, and updated its constructor
  - cleaned up code
- fixed
  - Odyssey 3D model importer, that broke after reading UVs and after reading certain models with SubmeshBlendShapeID data
  - Odyssey & Origins importers that dealt with Packed Entries
  - incorrect name of decompressed file while using `Tools > Decompress File`
  - models were flipped
  - LOD Selectors would use the icon for Materials
  - "Show in List" (or double-clicking a result) does not work while "<All Loaded Forges>" is selected in the Find window
  - Find was unable to search in DLC forges
  - the 3D Viewer would receive mouse events while the menu was open and mouse events occurred on the menu
  - subentries (entries within the forge file) that are numbers are now omitted
  - the experimental feature crashed Blacksmith

### Version 1.7.1
- (5/??/2019)
- This version was available for a short time, then was taken down.

### Version 1.7
- (5/24/2019)
- Added
  - 3DS, DAE, FBX (7.5 Binary), and STL (ASCII) export
  - partial Material support
  - "Remember last-used Find parameters", found in Settings
  - added a Multifile Entry will display its contents, the contents will display their subentries (this eases extracting embedded materials, models, etc. without opening the Multifile Entry Exporter)
  - proper UV support for Odyssey
  - Flip X & Flip Y options in the Save As window for textures (flips textures upon conversion)
  - link to external documentation
  - "Entity", "Material", and "Texture Set" to "Filter By" in the Find dialog
  - icons from the Visual Studio 2017 Image Library to the treeview
  - "Welcome to Blacksmith", a setup wizard that will show to first-time users
  - "Donate/Support this Project" dialog and "More > Donate/Support this Project" menu
  - "Experimental Features" option in the Settings (you should try it out!)
  - safeguard to protect against incorrect .ini files from being loaded in the Settings
  - "(Debug)/(Release)" label in the "About" dialog, indicating that I released a development version or a final version
  - the Aspose.3D library
- Updated
  - the teapot model is no longer included in Blacksmith - a cube is the new default model
  - major 3D Viewer improvements
    - now Blacksmith renders the model with a texture (instead of displaying the normals)
    - new controls: (added mouse support)
      - left-click drag: rotate model
      - right-click drag: move model
      - scroll wheel: zoom model
      - hold "Left Shift" key + W/A/S/D - move camera faster
      - hold "Left Shift" key + scroll wheel = zoom model faster
      - "R" key: reset camera and model transformation
      - "Q" key: move camera down
      - "E" key: move camera up
    - wireframe mode is proper (now renders triangles instead of quads)
    - camera adjustment fixes
    - adjust mouse sensitivity in the Settings
  - new temporary file structure
  - significantly improved Odyssey's and Origins' 3D model importers
  - "Show File in the Viewer" now scales down the 3D model
  - exported model scale
  - exported models (in OBJ) will contain groups with the model's name
  - AC texture importers
  - better detection and DDS generation
  - the Find window
  - a success message shows if the user saves the texture as a DDS
  - "Combine Meshes into Single File" is invisible (I did this because Aspose.3D has a limitation to saving multiple scenes at once)
  - visible meshes in the 3D Viewer will be the only meshes exported
  - results count is now comma-separated
  - time taken displays the total search time in seconds
  - "Toggle Alpha/Transparency" is now in the form of a dropdown with White as a new background color
  - NLog and ZStandard.Net versions
  - removed TextRenderer
  - About window
  - removed the DAE and DAEWriter classes
  - numerous other internal changes
- Fixed
  - the mesh scaling issue
  - incorrect model rotation
  - recompiled texconv (as Release and not Debug this time) - no more DLL errors
  - typos in the popups after saving raw and decompressed data
  - the context menu of `forge` entries will no longer display all menus
  - a default temp path 

### Version 1.6
- (4/20/2019)
- Added
  - Texture Set support, data will display in the Text Viewer
  - "Multifile Entry" support, any entry that contains additional files (Build Table, LOD Selector, to name a couple)
    - **found by right-clicking an entry or subentry and selecting `Multifile Entry > Extract Contained Files`, which reveals the Multifile Entry Exporter**
  - `Tools > Show File in the Viewers` and `Tools > Export 3D Viewer Contents`; both features are self-explanatory
  - number of results displays in the Find window
  - `Filter By` in the Find window (easily locate 3D models and textures)
  - automatic normal map fixing feature (flips blue channel; credit to @cire992), can be turned off in the Settings
  - save & load Settings to .ini files
  - 500+ new Resource Identifiers
  - if the entry is under 2 MB (compressed), all located Resource Identifiers will be displayed; otherwise, only the first Resource Identifier will be displayed
    - **note: extraction may take slightly longer as a result**
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

### Version 1.5.1
- (4/1/2019)
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

### Version 1.5 (preview)
- (3/31/2019)
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

### Version 1.4
- (3/8/2019)
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

### Version 1.3.1
- (2/9/2019)
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

### Version 1.3
- (2/18/2019)
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

### Version 1.2
- (2/3/2019)
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

### Version 1.1
- (1/23/2019)
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

### Version 1.0
- (1/19/2019)
- Initial version
- Known issues:
  - 3D Viewer does not show anything, yet
  - Image Viewer does not support .dds, yet (it supports .png)
