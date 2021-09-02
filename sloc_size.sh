#!/bin/bash

FILES=`find . \( -type d -name Migrations -o -type d -name obj \) -prune -o -type f -iname "*.cs" | grep .cs`

SLOC=`cat $FILES | grep -vE '(^\s*//|^\s*#|^\s*$|^\s*..\s$)' | wc --bytes`

wc --bytes $FILES
echo "$SLOC total bytes of SLOC"

