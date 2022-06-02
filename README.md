# Kmer.NET

A .NET/C# implementation and translation of [ridgelab/Kmer-SSR](https://github.com/ridgelab/Kmer-SSR)

# System Requirements

1. For building and running `Kmer.NET.CLI`, your Windows/macOS/Linux device should support .NET 6. <br /> 
   System requirements are [here](https://github.com/dotnet/core/blob/main/release-notes/6.0/supported-os.md);
2. For building and running `Kmer.NET.Windows`, your Windows device should support Windows App SDK. <br /> 
   System requirements are [here](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/system-requirements#windows-app-sdk).

# Testing

1. Under `data/input` subdirectory places the input samples for the unit test project `Kmer.NET.Test`;
2. Under `data/expected_output` subdirectory places the expected output result for the unit test project `Kmer.NET.Test`;
3. As you run the tests in the unit test project, the testing code will generate output files under `data/actual_output` subdirectory.
   After comparing their content with the expected output files, these files will be deleted;
4. Testing data "Human Genome Resources at NCBI" are not included in the repository due to the file size.
   If you want to run this specific test, you can uncomment the corresponding line of code, then download the input file from [here](https://www.ncbi.nlm.nih.gov/projects/genome/guide/human/index.shtml).
   Download `GRCh38 - RefSeq Transcripts [fasta]`, unzip the compressed file, copy it to data/input, and everything will be all set. Expected output file is not affected.

# Notes

For some reasons the "Browse file" buttons in Kmer.NET.Windows are not enabled, we could make a fix later as Windows App SDK gets updated.
