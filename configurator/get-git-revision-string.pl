#!/bin/perl

my $rev = `/bin/git rev-parse --short=4 HEAD`;
chomp $rev;

my $changes_string = `/bin/git status --short`;
my @changes = split(/\n/, $changes_string);

my $ch = ''; 


foreach (@changes)
{
	my $line = $_;

	if ( $line =~ /^(..)\s/ )
	{
		my $status_code = $1;
		$status_code =~ s/ /_/g;
		my $found = 0;
		for (my $i = 0; $i <= length $ch; $i+=2)
		{
			if (substr($status_code, 0, 1) eq substr($ch, $i, 1) && 
				substr($status_code, 1, 1) eq substr($ch, $i + 1, 1))
			{
				$found = 1;
				break;
			}
			
		}
		if (0 == $found)
		{
			$ch .= $status_code; # comma separated: # . ',';
		}
	}
}

if ($ch ne "")
{
	# comma separated:
	# $ch =~ s/,$//;
	
	$ch = '.' . $ch . '-LC'; # "local changes"
}

print $rev . $ch;
