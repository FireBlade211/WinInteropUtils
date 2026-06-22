# Reparse Points
A file or directory can contain a **reparse point**, which is a collection of **user-defined data**. The format of this data is understood by the application which stores the data, and a **file system filter**, which you install to interpret the data and process the file. When an application sets a reparse point, it stores this data, plus a **reparse tag**, which uniquely identifies the data it is storing. When the file system opens a file with a reparse point, it attempts to find the file system filter associated with the data format identified by the reparse tag. If a file system filter is found, the filter processes the file as directed by the reparse data. If a file system filter is not found, the file open operation fails.

For example, reparse points are used to implement NTFS **file system links** and the Microsoft **Remote Storage Server** (RSS). RSS uses an administrator-defined set of rules to move infrequently used files to long term storage, such as tape or optical media. It uses reparse points to store information about the file in the file system. This information is stored in a stub file that contains a reparse point whose data points to the device where the actual file is now located. The file system filter can use this information to retrieve the file.

Reparse points are also used to implement mounted folders.

## Reparse point restrictions
The following restrictions apply to reparse points:

- Reparse points can be established for a directory, but the directory must be empty. Otherwise, the NTFS file system fails to establish the reparse point. In addition, you can create directories or files in a directory that contains a reparse point with the directory bit set, but not otherwise.

- Reparse points and extended attributes are mutually exclusive. The NTFS file system cannot create a reparse point when the file contains extended attributes, and it cannot create extended attributes on a file that contains a reparse point.

- Reparse point data, including the tag and optional **GUID**, cannot exceed **16 kilobytes**. Setting a reparse point fails if the amount of data to be placed in the reparse point exceeds this limit.

- There is a limit of **63 reparse points** on any given path.

> [!NOTE]
> The limit can be reduced depending on the length of the reparse point. For example, if your reparse point targets a fully qualified path, the limit becomes 31.

