import os

path = "2019-02-25 [09][29][52]"

for dir1 in os.listdir(path):
	for dir2 in os.listdir(path + "/" + dir1):
		for file in os.listdir(path + "/" + dir1 + "/" + dir2):
			print(dir1 + "/" + dir2 + "/" + file)