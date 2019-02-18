#!/bin/bash

#SBATCH --job-name=ChessDTS_Worker
#SBATCH --nodes=1 --ntasks-per-node=1
#SBATCH --time=1-20:00:00
#SBATCH --mem=9GB
#SBATCH --output=worker_%A_%a.out
#SBATCH --array=0-599

module purge
module load mono/intel/4.8.0

srun mono GamePlayer.exe $SLURM_ARRAY_TASK_ID Level-0 200 3