# BinaryBrute
A brute force tool used to calculate the value of one or more hashes at the same time specifying the byte range using the processor power or through a dictionary, of which a list of hashes can be supplied to the input file **'hashes.txt'**, which will be searched through brute force specifying the algorithm.

This tool was developed for the purpose of evaluating how strong and secure passwords or keys are in a database, such as the Windows SAM database, which uses NTML hashes.

***The developer is not responsible for the improper or malicious use that they give to the tool***

## Usage
1.- Create a file called 'hashes.txt' that contains all the hashes to be calculated in hexadecimal format separated by line break.

2.- Start the application.

3.- Select the Algorithm to calculate the hashes.

4.- Select the way to generate byte entries to calculate the hashes. (Option 6 for worlist that needs a file called 'wordlist.txt' <with the keys separated by line breaks> in the folder containing the executable)
5.- And that's it.

**Note:** Every minute the program will save its current state, in case it is interrupted, you can return to where you left off by selecting the load state option when executing the program. If you want to save two different states, you can backup the file 'saved.txt', which saves the current state of the operation. 
