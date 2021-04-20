@echo off
if not exist "libraries" mkdir libraries

move de libraries\
move *.dll libraries
move *.pdb libraries
move libraries\Orcus.Administration.pdb Orcus.Administration.pdb
move *.xml libraries
move *.config libraries
move libraries\Orcus.Administration.exe.config Orcus.Administration.exe.config