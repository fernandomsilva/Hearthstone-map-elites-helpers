#!/bin/sh
#$ -N ProducerEvoMeta
#$ -S /bin/sh
#$ -cwd
#$ -j y
#$ -q medium
#$ -l mem_free=14.0G
/bin/echo Running on host: `hostname`.
/bin/echo In directory: `pwd`
/bin/echo Starting on: `date`

module load anaconda

python "/home/a/ahoover/EvoMeta/multiObjectiveEvolutionProducer.py" results-multiobj 300 3
