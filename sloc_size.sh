#!/bin/bash

FILES=`find . \( -type d -name Migrations -o -type d -name obj \) -prune -o -type f -iname "*.cs" | grep .cs`

SLOC=`cat $FILES | grep -vE '(^\s*$|^\s*..\s$)' | wc --bytes`
SLOC_NO_COMMENTS=`cat $FILES | grep -vE '(^\s*//|^\s*#|^\s*$|^\s*..\s$)' | wc --bytes`

wc --bytes $FILES | pr -t -2 -w `tput cols`
#echo -e "\e[1A\e[15C" "in" `echo "$FILES" | wc --lines` "files"
#echo "$SLOC total bytes of SLOC"
#echo "$SLOC_NO_COMMENTS total bytes of SLOC (without comments)"

