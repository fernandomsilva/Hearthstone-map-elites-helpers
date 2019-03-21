import os

def aggregateToVector(data, vectors):
	for i in range(0, len(data)):
		if len(vectors) <= i:
			vectors.append([[0, 0] for x in data[i]])
		
		for j in range(0, len(data[i])):
			vectors[i][j][0] += data[i][j][0]
			vectors[i][j][1] += data[i][j][1]

def parseWorkerFiles(filepath, number_of_matchups):
	files = [name for name in os.listdir(filepath + "/worker_data") if os.path.isfile(filepath + "/worker_data/" + name)]
	
	file_data = []
	result_vectors = []
	
	for file in files:
		print(file)
		input_file = open(filepath + "/worker_data/" + file, 'r')
		
		count = 0
		temp = []
		for line in input_file:
			if line != "" and line != "\n":
				line_elements = line.split(';')
				temp.append((int(line_elements[1]), int(line_elements[0])))
				
				count += 1
				if count >= number_of_matchups:
					file_data.append(temp)
					count = 0
					temp = []
		
		input_file.close()
		
		aggregateToVector(file_data, result_vectors)
		file_data = []
	
	vectors = []
	
	for i in range(0, len(result_vectors)):
		vectors.append([])
		for j in range(0, len(result_vectors[i])):
			vectors[i].append(float(result_vectors[i][j][0]) / float(result_vectors[i][j][1]))

	return vectors

#print(parseWorkerFiles("test/gen0", 3))