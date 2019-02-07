import os
import json

file_to_card_map = {}
path = "data/"
output_filename = "visualization.html"

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
		temp = json.loads(temp)
		
		temp_key = filename.split("_")
		temp_key = temp_key[1] + " " + temp_key[2] + " " + temp_key[3][:-4]
		
		if temp_key not in file_to_card_map:
			file_to_card_map[temp_key] = []
		file_to_card_map[temp_key].append({'cards': temp, 'score': row.split(";")[1]})
	data_file.close()
	
html_code = "<head>Apriori Results</head><body><p><p>"

for experiment in file_to_card_map.keys():
	html_code = html_code + "<b>" + experiment.upper() + "</b><p>"
	
	for entry in file_to_card_map[experiment]:
		html_code = html_code + "<font size=\"+5\">" + entry['score'] + "</font><p>"
		for card in entry['cards']:
			html_code = html_code + "<img src='images/" + card.replace("'", "-") + ".png' width=\"10%\"/>"
		
		html_code = html_code + "<p>"

html_code = html_code + "</body>"

output_file = open(output_filename, 'w')
output_file.write(html_code)
output_file.close()