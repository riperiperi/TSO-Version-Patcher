# TSO-Version-Patcher
.NET Core filesystem patcher based off of BsDiff. For patching TSO v1.1239.1.0 back to N&amp;I, but can be used for anything. 

Did this in case http://largedownloads.ea.com/ goes down forever. At the time of wrtiting, it is currently offline.

Included is a patch for the version of TSO available from web.archive.org. This should convert it back into the version FreeSO expects.

## Patching

Pretty simple really. Provide a patch file, and a source and destination directory. Unchanged files will be copied from the source to the destination, and patch/add/deletes will be applied appropriately.

Note that the destination can be the same as the source - patched files will simply overwrite. Note that there isn't really any sanity checking for this - you should make sure you're patching a cleanly extracted version of the game or it might terminate mid operation.

Example Command (framework version):

`TSOVersionPatcherF.exe 1239toNI.tsop PathToExtractedCabBase/ PathToDestination/`

Example Command (core version, in correct folder):

`dotnet run 1239toNI.tsop PathToExtractedCabBase/ PathToDestination/`

You can even shorten if you're extracting to the same folder:

`TSOVersionPatcherF.exe 1239toNI.tsop PathToInplacePatch/`


Failure returns exit code 1. Success returns 0.

## Making Patches

Example Command (framework version):
`TSOVersionPatcherF.exe --generate 1239toNI.tsop PathToVersionToBeConverted/ PathToTargetVersion/`

Example Command (core version, in correct folder):
`dotnet run --generate  1239toNI.tsop PathToVersionToBeConverted/ PathToTargetVersion/`
