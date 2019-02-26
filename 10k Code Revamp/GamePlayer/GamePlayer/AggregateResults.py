import sys, os

results = {}
path = ""

if len(sys.argv) < 2:
	print("You must pass the results main folder path as an argument to the script!")
	exit()
else:
	if not os.path.isdir(sys.argv[1]):
		print("Folder " + sys.argv[1] + " does not exist!")
		exit()
	
	path = sys.argv[1]

output_winRate = path + "/winRate.csv"
output_cardData = path + "/cardData.csv"

for dir1 in os.listdir(path):
	if os.path.isdir(path + "/" + dir1):
		results[dir1] = {}
		for dir2 in os.listdir(path + "/" + dir1):
			if os.path.isdir(path + "/" + dir1 + "/" + dir2):
				results[dir1][dir2] = {"numGames": 0, "numWins": 0, "cardData": {}}
				for file in os.listdir(path + "/" + dir1 + "/" + dir2):
					if os.path.isfile(path + "/" + dir1 + "/" + dir2 + "/" + file):
						entry = results[dir1][dir2]
					
						file_data = open(path + "/" + dir1 + "/" + dir2 + "/" + file, 'r')
						
						temp = file_data.readline()
						temp = temp.split(';')
						
						entry["numGames"] += int(temp[0])
						entry["numWins"] += int(temp[1])
						
						for line in file_data:
							if ';' in line:
								key, value, empty = line.split(';')
								
								if key not in entry["cardData"]:
									entry["cardData"][key] = int(value)
								else:
									entry["cardData"][key] += int(value)
						
						file_data.close()
				
				print(path + "/" + dir1 + "/" + dir2 + " DONE!")

winRate_output_file = open(output_winRate, 'w')

column_header = ","
for key in results[list(results.keys())[0]]:
	column_header += key + ","
column_header = column_header[:-1] #remove the extra comma
winRate_output_file.write(column_header + '\n')

for k1 in results:
	row = k1 + ","
	for k2 in results[k1]:
		row += str(float(results[k1][k2]["numWins"] / results[k1][k2]["numGames"])) + ","
		print((results[k1][k2]["numWins"], results[k1][k2]["numGames"]))
	
	row = row[:-1]
	winRate_output_file.write(row + '\n')

winRate_output_file.close()

cardData_output_file = open(output_cardData, 'w')

column_header = "deck,card,number of times played"
cardData_output_file.write(column_header + '\n')

for k1 in results:
	result_dict = {}
	for k2 in results[k1]:
		for key in results[k1][k2]["cardData"]:
			if key not in result_dict:
				result_dict[key] = results[k1][k2]["cardData"][key]
			else:
				result_dict[key] += results[k1][k2]["cardData"][key]
	
	#Printing results ordered by value
	dict_view = [ (v,k) for k,v in result_dict.items() ]
	dict_view.sort(reverse=True)
	for value, card in dict_view:
		cardData_output_file.write(k1 + "," + card + "," + str(result_dict[card]) + '\n')

cardData_output_file.close()
