# Expects shell to expand *.amf etc.

print $#ARGV + 1 . "\n";

if($#ARGV + 1 < 1) {
  die "Not enough args";
}

foreach $file (@ARGV) {
  print $file . "\n---\n";
  system("cat $file | /cygdrive/c/Program\\\ Files\\\ \\\(x86\\\)/Aspell/bin/aspell.exe list");
#  system("cat $file");
  print "\n\n";
}
