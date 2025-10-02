#!/bin/bash

echo "=== Silkworm Installation Verification ==="
echo ""

GHA_PATH="/Users/josh/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper (b45a29b1-4343-4035-989e-044e8580d9cf)/Libraries/Silkworm.gha"
BUILD_PATH="/Users/josh/dev/grasshopper/Silkworm/bin/Debug/Silkworm.gha"

echo "1. Checking build file..."
if [ -f "$BUILD_PATH" ]; then
    echo "   ✅ Build file exists"
    ls -lh "$BUILD_PATH"
else
    echo "   ❌ Build file NOT found"
fi

echo ""
echo "2. Checking installed file..."
if [ -f "$GHA_PATH" ]; then
    echo "   ✅ Installed file exists"
    ls -lh "$GHA_PATH"
else
    echo "   ❌ Installed file NOT found"
fi

echo ""
echo "3. Comparing file hashes..."
BUILD_MD5=$(md5 -q "$BUILD_PATH")
INSTALL_MD5=$(md5 -q "$GHA_PATH")

echo "   Build MD5:     $BUILD_MD5"
echo "   Installed MD5: $INSTALL_MD5"

if [ "$BUILD_MD5" = "$INSTALL_MD5" ]; then
    echo "   ✅ Files match - installation is up to date!"
else
    echo "   ❌ Files DO NOT match - need to copy new version"
    echo ""
    echo "   Run this command to update:"
    echo "   cp \"$BUILD_PATH\" \"$GHA_PATH\""
fi

echo ""
echo "4. Checking for new components in build..."
cd /Users/josh/dev/grasshopper/Silkworm/AnalyzeSilkworm2
dotnet run 2>&1 | grep -A 1 "^Component: SilkwormCompiler\|^Component: FlowCalculator"

echo ""
echo "=== Next Steps ==="
echo "1. Quit Rhino completely"
echo "2. Restart Rhino"
echo "3. Open Grasshopper"
echo "4. Look for 'Flow Calculator' and 'Silkworm Compiler' in Silkworm tab"
echo ""
