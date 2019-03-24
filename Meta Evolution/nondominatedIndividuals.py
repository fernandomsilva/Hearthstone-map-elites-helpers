import numpy

def getNondominatedIndividuals (pop):
	#pop is a list of 3-tuples where:
	# the first value of an individual is its chromosome (irrelevant for our purpose)
	# the second value is its meta-balance fitness
	# the third value is its cost-fitness

	fronts = []

	while (pop):

		nondominatedIndividuals = []
		nondominatedIndices = []

		

		for i in range(len(pop)):
			# Outer loop
			fit1 = pop[i][1]
			fit2 = pop[i][2]


			nondominated = True
			for j in range(len(pop)):
				if (nondominated == False):
					break
				if (i != j):
					otherFit1 = pop[j][1]
					otherFit2 = pop[j][2]

					if ( (fit1 > otherFit1 and fit2>= otherFit2) or (fit2>otherFit2 and fit1>=otherFit1)):
						nondominated = False

			if(nondominated == True):
				nondominatedIndividuals.append(pop[i])
				nondominatedIndices.append(i)

		fronts.append(nondominatedIndividuals)
		for index in nondominatedIndices:
			del pop[index]

	return fronts


def main():
	pop = [[0,10,10],[1,2,3],[2,3,2],[3,5,7],[4,5,8]]
	pop = sorted(pop, key=lambda ind: ind[1]) #sort population by first fitness dimension
	print(pop)
	fronts = getNondominatedIndividuals(pop)
	for front in fronts:
		print(front)

if __name__ == '__main__':
	main()





					

