#!/bin/bash

echo "Listing files... (might take a moment on win)"
files=`find . -iname "*.cs"`
echo "Replacing CRLF with LF..."
longest=`echo "$files" | wc -L | cut -d' ' -f 1`
for file in `find . -iname "*.cs"`; do
	count=`cat "$file" | tr '\r' '#' | grep "#" | wc -l`
	if (( "$count" > 0 )); then
		printf "%-${longest}s %s occurences\n" "$file" "$count"
		sed -i 's/\r\n$/\n/' $file
	fi
done


