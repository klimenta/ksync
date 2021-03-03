ksync is a small utility that syncs two directories.

Type ksync without any parameters for a list of options.

There are two sync options, echo and mirror. If you don't specify the -o option, echo is the default.
--- Echo copies the missing files and the newer files (if destination file exists) from the source to the destination. (A -> B).
--- Mirror does the same, but then checks the destination against source. (A -> B, B -> A).
There is no deletion of any files in both echo and mirror sync options.
If you delete a file at the source, the same file won't be deleted at the destination.
Always run the initial sync with -t or --test option to see what's going to happen.
If you more granularity, use -c or --confirm option. It will prompt to copy each file at the destination.
Do not copy (N) is the default option, so hitting enter will skip the file.
You can use an exception list if you want to skip some files or directories.
Don't use wildcards in the exception list for directories. Wildcard * is for file extensions only.
For example if you want to skip all directories that have the word temp in the name, use -e temp.
This option will skip "temporary Files", "Some temp dir", "my temp" etc.
Use comma delimited separator for more exceptions, e.g. -e *.jpg,*.tmp,temp. Use wildcards to skip file extensions.
If you want to skip system or hidden files or directories, use -ss and -sh options.
