#!/bin/sh
printf '\033c\033]0;%s\a' Faeterna
base_path="$(dirname "$(realpath "$0")")"
"$base_path/Faeterna_win" "$@"
