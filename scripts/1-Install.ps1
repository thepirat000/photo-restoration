# Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy Bypass -Force;
Set-ExecutionPolicy Bypass -Scope Process -Force; 

# Install chocolatey
iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'));

# GIT
Write-Host "Installing GIT" -foregroundcolor "green";
choco install git -y --no-progress

# CUDA drivers
Write-Host "Installing CUDA drivers (this can take some time)" -foregroundcolor "green";
choco install cuda --ignore-checksums -y --no-progress

refreshenv

# Install Conda
Write-Host "Installing miniconda3 (this can take some time)" -foregroundcolor "green";
choco install miniconda3 -y --no-progress

& 'C:\tools\miniconda3\shell\condabin\conda-hook.ps1'; 
conda activate 'C:\tools\miniconda3';

conda update -n base -c defaults conda -y

conda create -n PHOTO python=3.7 conda -y

# Install 3d-photo
conda activate PHOTO

conda install pytorch==1.4.0 torchvision==0.5.0 cudatoolkit==10.1.243 -c pytorch -y

conda deactivate 

conda deactivate 


# Install NVIDIA drivers and run: 
Write-Host "Installing NVIDIA drivers" -foregroundcolor "green";
$file = $PSScriptRoot + '\442.50-tesla-desktop-win10-64bit-international.exe';
Start-BitsTransfer -Source http://us.download.nvidia.com/tesla/442.50/442.50-tesla-desktop-win10-64bit-international.exe -Destination $file
& $file
& "C:\Program Files\NVIDIA Corporation\NVSMI\nvidia-smi" -fdm 0

refreshenv

# Pre-req  

mkdir C:\Util
CD C:\Util
Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force

## Install VC++
$url  = 'https://download.visualstudio.microsoft.com/download/pr/9d2147aa-7b01-4336-b665-8fe07735e5ee/2706b53b5d7d6e8d12937b637648481eb45561a06f8fabef247350ed5233befa/vs_Community.exe';
$file = '.\vs_Community.exe';
Start-BitsTransfer -Source $url -Destination $file
& $file --wait --add Microsoft.VisualStudio.Workload.NativeDesktop --add Microsoft.VisualStudio.Workload.Python --add Microsoft.VisualStudio.Component.VC.CMake.Project --passive

$url  = 'https://download.visualstudio.microsoft.com/download/pr/9d2147aa-7b01-4336-b665-8fe07735e5ee/c4b2212532e637ae35783660e79211e0604d0b61e54f6c7db69df30ce446bd73/vs_BuildTools.exe';
$file = '.\vs_BuildTools.exe';
Start-BitsTransfer -Source $url -Destination $file
& $file --wait --add Microsoft.VisualStudio.Workload.VCTools --add Microsoft.VisualStudio.Component.VC.CMake.Project --passive

## Bzip2
$url  = 'https://versaweb.dl.sourceforge.net/project/gnuwin32/bzip2/1.0.5/bzip2-1.0.5-bin.zip';
$file = '.\bzip2-1.0.5-bin.zip';
Start-BitsTransfer -Source $url -Destination $file
Expand-Archive $file

# Install Bringing-Old-Photos-Back-to-Life
mkdir c:\GIT
cd c:\GIT

git clone https://github.com/microsoft/Bringing-Old-Photos-Back-to-Life.git
cd Bringing-Old-Photos-Back-to-Life

## Clone the Synchronized-BatchNorm-PyTorch repository

cd Face_Enhancement/models/networks/
rm .\Synchronized-BatchNorm-PyTorch -Recurse -Force -Confirm:$false
git clone https://github.com/vacancy/Synchronized-BatchNorm-PyTorch
cp -Force -Recurse Synchronized-BatchNorm-PyTorch/sync_batchnorm .
cd ../../../

cd Global/detection_models
git clone https://github.com/vacancy/Synchronized-BatchNorm-PyTorch
cp -Force -Recurse Synchronized-BatchNorm-PyTorch/sync_batchnorm .
cd ../../

## Download the landmark detection pretrained model

cd Face_Detection/

$url  = 'http://dlib.net/files/shape_predictor_68_face_landmarks.dat.bz2';
$file = '.\shape_predictor_68_face_landmarks.dat.bz2';
Start-BitsTransfer -Source $url -Destination $file
C:\Util\bzip2-1.0.5-bin\bin\bunzip2.exe .\shape_predictor_68_face_landmarks.dat.bz2
cd ../

cd Face_Enhancement/
$url  = 'https://facevc.blob.core.windows.net/zhanbo/old_photo/pretrain/Face_Enhancement/checkpoints.zip';
$file = '.\checkpoints.zip';
Start-BitsTransfer -Source $url -Destination $file
Expand-Archive $file .\
cd ../

cd Global/
$url  = 'https://facevc.blob.core.windows.net/zhanbo/old_photo/pretrain/Global/checkpoints.zip';
$file = '.\checkpoints.zip';
Start-BitsTransfer -Source $url -Destination $file
Expand-Archive $file .\
cd ../

conda activate PHOTO
pip install cmake
conda install -y -c conda-forge dlib
pip install -r requirements.txt
conda deactivate