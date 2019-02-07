import os
import urllib.request
import json

cards = set([])
path = "data/"

for filename in os.listdir(path):
	data_file = open(path + filename)
	for row in data_file:
		temp = row.split(";")[0]
		temp = temp.replace("(", "[")
		temp = temp.replace(")", "]")
		temp = temp.replace("',", "\",")
		temp = temp.replace(" '", " \"")
		temp = temp.replace("['", "[\"")
		temp = temp.replace("']", "\"]")
		if temp[-2] == ",":
			temp = temp[:-2] + temp[-1]
		temp = json.loads(temp)
		cards = cards.union(set(temp))
	data_file.close()

cards = list(cards)

url = "https://raw.githubusercontent.com/schmich/hearthstone-card-images/4.12.2/rel/"

db_file = open("cards.json", encoding="utf-8")
data = json.load(db_file)
db_file.close()

card_id_map = {}

for row in data:
	if 'dbfId' in row:
		card_id_map[row['name']] = str(row['dbfId'])

for card in cards:
	try:
		if not os.path.isfile("images/" + card + ".png"):
			urllib.request.urlretrieve(url + card_id_map[card] + ".png", "images/" + card + ".png")
		print(card, " done!  (" + str(cards.index(card) + 1) + "/" + str(len(cards)) + ")")
	except:
		print(card, " ERROR!  (" + str(cards.index(card) + 1) + "/" + str(len(cards)) + ")")
