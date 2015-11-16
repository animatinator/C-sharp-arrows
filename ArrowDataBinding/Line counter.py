import os

lines = 0
linesextended = 0
comments = 0
whitespace = 0
files = os.listdir(os.getcwd())
allowedextensions = ["cs"]
excluded = []

for root, subFolders, fileList in os.walk(os.getcwd()):
    for filef in fileList:
        files.append(os.path.join(root,filef))

print "     --ExeSoft line counter--"
print
print "Counting lines in all files with the following extensions:"

for extension in allowedextensions:
    print "\t*.%s" % extension

print
print "Excluding the following files:"

for excludedfile in excluded:
        print "\t%s" % excludedfile
print
print "Counting..."

for item in files:
    extension = item.split(".")[len(item.split(".")) - 1]
    if (extension in allowedextensions) and (item not in excluded) and (item != "Line counter.py"):
        opened = open(item)
        print "\tCounting in \'%s\'..." % item
		
        totalcount = 0
        totalextended = 0
        for line in opened.readlines():
                temp = line.replace("\t", "")
                temp = temp.replace(" ", "")
                temp = temp.replace("\n", "")
                if (temp != "") and ("".join(temp[0:2]) != "//"):
                        totalcount += 1
                        totalextended += 1
                else:
                        if temp == "":
                            whitespace += 1
                        elif "".join(temp[0:2]) == "//":
                            comments += 1
                        totalextended += 1
		
        lines += totalcount
        linesextended += totalextended
        opened.close()

print
print "Done!"
raw_input()
print
print
print "Line counts:\n\t-Excluding comments and whitespace: %i\n\t-Everything: %i" % (lines, linesextended)
raw_input()
print
print "Extras:\n\t-Number of comments: %i\n\t-Number of blank lines: %i" % (comments, whitespace)
raw_input()
print
codepercent = (float(lines) / float(linesextended)) * 100.0
commentpercent = (float(comments) / float(linesextended)) * 100.0
whitespacepercent = (float(whitespace) / float(linesextended)) * 100.0
print "Code breakdown:"
print "\t-Functional code: %f%%\n\t-Comments: %f%%\n\t-Whitespace: %f%%" % (codepercent, commentpercent, whitespacepercent)
raw_input()
