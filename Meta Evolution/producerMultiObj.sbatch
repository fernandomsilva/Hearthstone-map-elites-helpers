#!/bin/bash
#
#SBATCH --job-name=Producer_MultiOBJ
#SBATCH --nodes=1
#SBATCH --ntasks-per-node=1
#SBATCH --cpus-per-task=1
#SBATCH --time=40:00:00
#SBATCH --mem=2GB
#SBATCH --mail-type=END
#SBATCH --mail-type=BEGIN
#SBATCH --mail-user=fdm240@nyu.edu

module purge
module load python3/intel/3.6.3

pip3 install --user numpy
pip3 install --user deap

python multiObjectiveEvolutionProducer.py results-multiobj 300 3
