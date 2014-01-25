# Rearranges dwarf fortress tile ordering for use in libtcod
# requires ImageMagick

my $imageMagickPath="C:/Program\\\ Files/ImageMagick-6.8.8-Q16/";

use File::Find;

sub rename_files_with_leading_zeros
{
    my $file = $_;
    if($file =~ /.*_(\d+)\.png/) {
	print "cp " . $file . " output_tiles_" . sprintf("%03d", $1) . ".png\n";
	system("cp " . $file . " output_tiles_" . sprintf("%03d", $1) . ".png");
    }
}

my $fileToConvert=$ARGV[0];
my $inputTileSize=$ARGV[1];

my $outputFilename="output.png";

if($#ARGV < 1) {
    die("Usage: input-filename input-tile-size [square]");
}

system("rm output_tiles_*");
system("rm crawl_tiles_*");

# Splice master PNG into individual icons

#my $fileNamingConvention = " -set filename:tile '%[fx:page.x/" . $inputTileSize . "+1+fx:page.y/" . $inputTileSize . "+1]'";
#my $fileNamingConvention = " -set filename:tile '%[fx:page.x/10+16*(page.y/10)]'";
my $fileNamingConvention = " -set filename:tile '%[fx:16*(page.x/10)+page.y/10]'";
my $cropping = " -crop " . $inputTileSize . "x" . $inputTileSize;

my $tileCmd = $imageMagickPath . "convert.exe " . $fileToConvert . $cropping . $fileNamingConvention . " +repage +adjoin 'tiles_crawl_%[filename:tile].png'";

#my $tileCmdWorks = $imageMagickPath . "convert.exe " . $fileToConvert . $fileNamingConvention . $cropping . " +repage +adjoin 'tiles_crawl_%03d.png'";
my $reassembleCmd = $imageMagickPath . "montage.exe -mode concatenate -background none -tile 16x output_tiles_*.png ". $outputFilename;

print $tileCmd . "\n";

system($tileCmd);

# annoyingly we don't get leading zeros, so we need to do a conversion

find(\&rename_files_with_leading_zeros, ".");

print $reassembleCmd . "\n";

system($reassembleCmd);

print "Wrote " . $outputFilename . "\n";
