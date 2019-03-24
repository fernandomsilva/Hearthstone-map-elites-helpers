#!/bin/bash
#
#SBATCH --job-name=Producer_MultiOBJ
#SBATCH --nodes=1
#SBATCH --ntasks-per-node=1
#SBATCH --cpus-per-task=1
#SBATCH --array=0-399
#SBATCH --time=40:00:00
#SBATCH --mem=5GB
#SBATCH --mail-type=END
#SBATCH --mail-type=BEGIN
#SBATCH --mail-user=fdm240@nyu.edu

module purge
module load mono/intel/4.8.0

srun mono evolutionWorker.exe gpuid=$SGE_TASK_ID numGames=1 folder=results-multiobj playerdecks=player_decks.csv opponentdecks=player_decks.csv maxwidth=12 maxdepth=3 numworkers=300

