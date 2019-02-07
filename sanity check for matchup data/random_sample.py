import csv
import random
import numpy as np

file_data = csv.reader(open("Deck2-0.csv"))

data = []

for row in file_data:
	if row[0][0:5] == "GPUID":
		if 'Player1: WON' in row[2]:
			data.append(1)
		elif 'Player1: LOST' in row[2]:
			data.append(0)

print("Full win-rate: " + str(sum(data) / len(data)))

rand_sample_results = []

for i in range(0, 30):
	rand_sample = random.sample(data, 34)
	rand_sample_results.append(sum(rand_sample) / len(rand_sample))
	print("Random Sample " + str(i) + ": " + str(rand_sample_results[-1]))

print("Standard Dev: " + str(np.std(rand_sample_results)))
print("Variance: " + str(np.var(rand_sample_results)))