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
								key, matches_played, win_when_played, matches_played_twice, win_when_played_twice, draws, win_when_drawn, empty = line.split(';')
								
								if key not in entry["cardData"]:
									#entry["cardData"][key] = int(value)
									entry["cardData"][key] = {"matches_played": int(matches_played), "win_when_played": int(win_when_played), "matches_played_twice": int(matches_played_twice), "win_when_played_twice": int(win_when_played_twice), "draws": int(draws), "win_when_drawn": int(win_when_drawn) }
								else:
									#entry["cardData"][key] += int(value)
									entry["cardData"][key]["matches_played"] += int(matches_played)
									entry["cardData"][key]["win_when_played"] += int(win_when_played)
									entry["cardData"][key]["matches_played_twice"] += int(matches_played_twice)
									entry["cardData"][key]["win_when_played_twice"] += int(win_when_played_twice)
									entry["cardData"][key]["draws"] += int(draws)
									entry["cardData"][key]["win_when_drawn"] += int(win_when_drawn)
						
						file_data.close()
				
				print(path + "/" + dir1 + "/" + dir2 + " DONE!")

winRate_output_file = open(output_winRate, 'w')

list_of_decks = set()

for key in results:
	list_of_decks.add(key)
	for k2 in results[key]:
		list_of_decks.add(k2)

for deck in list_of_decks:
	if deck not in results:
		results[deck] = {}

order_to_print = []
temp = []

for deck in list_of_decks:
	temp.append((deck, sum([results[deck][x]["numGames"] for x in results[deck]])))

temp = sorted(temp, key=lambda k:k[1], reverse=True)
order_to_print = [temp[i][0] for i in range(0, len(temp))]

for key in results:
	for deck in list_of_decks:
		if deck not in results[key]:
			results[key][deck] = {"numGames": -1, "numWins": 0, "cardData": {}}

column_header = ","
#for key in results[list(results.keys())[0]]:
for key in order_to_print:
	column_header += key + ","
column_header = column_header[:-1] #remove the extra comma
winRate_output_file.write(column_header + '\n')

for k1 in column_header[1:].split(','):
	row = k1 + ","
	for k2 in column_header[1:].split(','):
		if results[k1][k2]["numGames"] == -1:
			row += ","
		else:
			row += str(float(results[k1][k2]["numWins"] / results[k1][k2]["numGames"])) + ","
		print((results[k1][k2]["numWins"], results[k1][k2]["numGames"]))
	
	row = row[:-1]
	winRate_output_file.write(row + '\n')

winRate_output_file.close()

cardData_output_file = open(output_cardData, 'w')

column_header = "deck,card,# matches played,# wins when played,# matches played twice,# wins when played twice,# times drawn,# wins when drawn"
cardData_output_file.write(column_header + '\n')

for k1 in results:
	result_dict = {}
	for k2 in results[k1]:
		for key in results[k1][k2]["cardData"]:
			if key not in result_dict:
				result_dict[key] = {"plays": 0, "wins_play": 0, "play_twice": 0, "wins_played_twice": 0, "draws": 0, "win_draws": 0}
				result_dict[key]["plays"] = results[k1][k2]["cardData"][key]["matches_played"]
				result_dict[key]["wins_play"] = results[k1][k2]["cardData"][key]["win_when_played"]
				result_dict[key]["play_twice"] = results[k1][k2]["cardData"][key]["matches_played_twice"]
				result_dict[key]["wins_played_twice"] = results[k1][k2]["cardData"][key]["win_when_played_twice"]
				result_dict[key]["draws"] = results[k1][k2]["cardData"][key]["draws"]
				result_dict[key]["win_draws"] = results[k1][k2]["cardData"][key]["win_when_drawn"]
			else:
				result_dict[key]["plays"] += results[k1][k2]["cardData"][key]["matches_played"]
				result_dict[key]["wins_play"] += results[k1][k2]["cardData"][key]["win_when_played"]
				result_dict[key]["play_twice"] += results[k1][k2]["cardData"][key]["matches_played_twice"]
				result_dict[key]["wins_played_twice"] += results[k1][k2]["cardData"][key]["win_when_played_twice"]
				result_dict[key]["draws"] += results[k1][k2]["cardData"][key]["draws"]
				result_dict[key]["win_draws"] += results[k1][k2]["cardData"][key]["win_when_drawn"]

	
	#Printing results ordered by value
	dict_view = [ (result_dict[k]["plays"],k) for k in result_dict.keys() ]
	dict_view.sort(reverse=True)
	for value, card in dict_view:
		cardData_output_file.write(k1 + "," + card + "," + str(result_dict[card]["plays"]) + "," + str(result_dict[card]["wins_play"]) + "," + str(result_dict[card]["play_twice"]) + "," + str(result_dict[card]["wins_played_twice"]) + "," + str(result_dict[card]["draws"]) + "," + str(result_dict[card]["win_draws"]) + '\n')

cardData_output_file.close()
