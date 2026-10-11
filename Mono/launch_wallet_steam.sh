#!/bin/bash
# Preserve Steam's runtime arguments and insert BepInEx at the actual game.
set -eu
args=()
found=0
for arg in "$@"; do
    case "$arg" in
        */KingdomTwoCrowns.x86|KingdomTwoCrowns.x86)
            args+=("$(dirname "$arg")/run_bepinex.sh")
            found=1
            ;;
    esac
    args+=("$arg")
done
if [ "$found" != 1 ]; then
    echo 'Kingdom Two Crowns executable missing in Steam command.' >&2
    exit 1
fi
exec "${args[@]}"
