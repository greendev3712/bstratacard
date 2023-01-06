#!/usr/bin/perl
################################################################################
#
# ----
# --- Set up for New Customer Deployment
# ----
#
# 1) Add Customer Deployment to %DEPLOYMENTS
# 
# 2) Build the first version of deployment 
#    ./Deploy.pl -d <DeploymentName> -nv
#
# 3) Build Application setup.exe
#    ./Deploy.pl -d <DeploymentName> -bs
#    ** Upload application-setup.exe to customer's deployment url, ie: /var/www/software/intellaToolbar 
#
#
# ----
# -- Deploy an updated application
# ----
#
#   ./Deploy.pl -d <DeploymentName> -nv
#
#
################################################################################
#
# curl -L cpanmin.us | perl - -l$HOME/perl5 App::cpanminus local::lib Capture::Tiny IPC::System::Simple
# eval $(perl -I$HOME/perl5/lib/perl5 -Mlocal::lib)
#
################################################################################

use warnings;
use strict;

use feature 'say';

use File::Basename;

my $PROGRAM_NAME;

BEGIN {
  $PROGRAM_NAME = 'IntellaAppsUpdater';
};

my $SCRIPT_NAME  = basename($0);
$SCRIPT_NAME =~ s/^\.\///;
$0 = $SCRIPT_NAME;

use Data::Dumper;
use File::Slurp;
use File::stat;
use Getopt::Long;
use POSIX qw();

my $OP = '';
my $VERBOSE = 0;

my $DEPLOYMENT;
my $DEPLOYMENT_NAME;
my $DEPLOYMENT_BASE_PATH = 'deploy'; # = '/cygdrive/z/Projects/intellaQueue/deploy';
my $DEPLOYMENT_WYP;
my $DEPLOYMENT_UPDATES;
my $DEPLOYMENT_INSTALLER;
my $DEPLOYMENT_RSYNC_OPTS = qq{--exclude "*.wmv" --exclude "app.deploy"/};

my $CMD_WYBUILD = '"/cygdrive/c/Program Files (x86)/wyBuild/wybuild.cmd.exe"';
my $CMD_CC      = '/cygdrive/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/Common7/IDE/devenv.exe';

if (! -e $CMD_CC) {
  $CMD_CC =  '/cygdrive/c/Program Files (x86)/Microsoft Visual Studio/2019/Enterprise/Common7/IDE/devenv.exe';
}

$CMD_CC = qq{"$CMD_CC"};

my %APPS = (
  intellaQueue => {
    gitBase          => '/cygdrive/c/intellaApps',
    appBinary        => '/cygdrive/c/intellaApps/intellaQueue/src/intellaQueue/bin/Debug/intellaQueue.exe',
    # The rest of these are quoted so we can use them directly wyUpdate.exe 
    #  (which needs windows paths) without having to quote (in case in the future these paths might contain spaces or something)
    project          => '"c:\\intellaApps\\intellaQueueWithIntellaCast.sln"',
    setupProject     => '"c:\\intellaApps\\intellaQueue\\src\\SetupProject\\SetupProject.wixproj"',
    bootstrapProject => '"c:\\intellaApps\\intellaQueue\\src\\Bootstrapper\\Bootstrapper.wixproj"',
  }
);

my $DEVENV_REBUILD = 0;

################################################################################

# TODO: Support multi-site uploads.. ie: KWI-A/B  BSSB-A/B  etc etc

my %DEPLOYMENTS = (
  'vbox-markm' => {
     config_wyp     => 'IntellaQueue - VBOX-MarkM.wyp',
     default_server => 'vbox-markm.intellasoft.local',
  },

  'vbox-markm-64' => {
     config_wyp     => 'IntellaQueue - VBOX-MarkM-64.wyp',
     default_server => 'vbox-markm-64.intellasoft.local',
  },
    
  'intellasoft' => {
    config_wyp     => 'IntellaQueue - Intellasoft.wyp',
    default_server => 'comm1.nyc.intellasoft.net',
  },
    
  'acs-passaic' => {
     config_wyp     => 'IntellaQueue - ACS Passaic.wyp',
     default_server => '192.168.1.201',
  },
  'acs-passaic-remote' => {
     config_wyp     => 'IntellaQueue - ACS Passaic Remote.wyp',
     default_server => '10.20.1.1',
  },
  'acs-hudson' => {
     config_wyp     => 'IntellaQueue - ACS Hudson.wyp',
     default_server => '192.168.2.201',
  },
  'acs-hudson-remote' => {
     config_wyp     => 'IntellaQueue - ACS Hudson Remote.wyp',
     default_server => '10.20.2.1',
  },
  'acs-cin' => {
     config_wyp     => 'IntellaQueue - ACS Cin.wyp',
     default_server => '192.168.0.201',
  },
  'acs-cin-remote' => {
     config_wyp     => 'IntellaQueue - ACS Cin Remote.wyp',
     default_server => '10.20.0.1',
  },
  'kwi-a' => {
     config_wyp     => 'IntellaQueue - KWI.wyp',
     default_server => '',
  },
  # Intellasoft VUNYC1 / portal2.intellasoft.net
  'portal2' => {
     config_wyp     => 'IntellaQueue - Portal2.wyp',
     default_server => 'portal2.intellasoft.net',
  },

  # Specific to Pyramid Roofing at the moment
  'ch02.gtxit.com' => {
     config_wyp     => 'IntellaQueue - Pyramid.wyp',
     default_server => 'ch02.gtxit.com',
  },

  # The deployment is on kc02... but the voice service is on ch01... we should figure out a push to change the update server location
  'kc02.gtxit.com' => {
     config_wyp     => 'IntellaQueue - Apollo.wyp',
     default_server => 'kc02.gtxit.com',
  },
);

sub getCurrentAppVersion {
  my ($appName) = @_; # FIXME:  Not Used

  my $next_version  = '1.0'; # Will be updated if needed

  my @previous_deploys = sort glob("$DEPLOYMENT_UPDATES/*");
  my $current_deploy   = pop(@previous_deploys) // '0.0';
  
  my @current_deploy_path_parts = fileparse($current_deploy);
  # Example:
  # [
  #  '1.2',
  #  'deploy/acs/updates/',
  #  '',
  # ]

  my $current_version = $current_deploy_path_parts[0];

  if (! -d "$current_deploy/") {
    # New deployment entirely
    say "--> Detected brand new deployment";
    return ('0.0', '1.0');
  }
  
  # If we do have the current version deployed, then we're not an initial release
  if (-e "$current_deploy/version.txt") {
    $next_version = $current_version + 0.1;
  }
  else {
    if (-d $current_deploy) {
      say "!!! Could not determine next version.  Do we have a proper build in: $current_deploy ?";
      exit;
    }
  }
  
  return ($current_version, $next_version);
}

my $OptionsResult = GetOptions(
  "h" => sub {
    print "\nUsage:\n $SCRIPT_NAME <options>\n";
    print "  -h         This Help\n";
    print "  -v         Verbose\n";
    print "  -bs        ReBuild BootStrapper + SetupProject (with    /Rebuild) -- Rebuild required if we've made new upgrades for wyUpdate\n";
    print "  -bsnr      Build BootStrapper + SetupProject   (without /Rebuild) -- Rebuild not required if we're only making changes to Bootstrapper/SetupProject\n";
    print "  -nv        New Version -- Take the current build of intellaQueue and create and build a new version in wyUpdate\n";
    print "  -bwu       Build client.wyc\n";
    print "  -upload    (Re)Upload current build";
    print "\n";
    exit;
  },
  'v' => sub {
    $VERBOSE = 1;
  },
  'd=s' => sub {
    $DEPLOYMENT_NAME = $_[1] // '';

    say "";
    say "Deployment Name: $DEPLOYMENT_NAME";
    $DEPLOYMENT = $DEPLOYMENTS{$DEPLOYMENT_NAME} if ($DEPLOYMENT_NAME);

    if (!defined($DEPLOYMENT)) {
      say "!!! Deployment not found: $DEPLOYMENT_NAME (Hint: Edit $0 and add to %DEPLOYMENTS)";
      exit;
    }

    $DEPLOYMENT_BASE_PATH .= "/$DEPLOYMENT_NAME";
    $DEPLOYMENT_UPDATES    = $DEPLOYMENT_BASE_PATH . '/updates';
    $DEPLOYMENT_INSTALLER  = $DEPLOYMENT_BASE_PATH . '/installer';
    $DEPLOYMENT_WYP        = "wyUpdateProjects/$DEPLOYMENT->{config_wyp}";

    say "";
    say "DEPLOYMENT_BASE_PATH : $DEPLOYMENT_BASE_PATH";
    say "DEPLOYMENT_UPDATES   : $DEPLOYMENT_UPDATES   ";
    say "DEPLOYMENT_INSTALLER : $DEPLOYMENT_INSTALLER ";
    say "DEPLOYMENT_WYP       : $DEPLOYMENT_WYP       ";

    `mkdir -p "$DEPLOYMENT_UPDATES"   > /dev/null 2>&1`;
    `mkdir -p "$DEPLOYMENT_INSTALLER" > /dev/null 2>&1`;

    if (! -e $DEPLOYMENT_WYP) {
      say "!!! wyUpdate Project does not exist: $DEPLOYMENT_WYP";
      exit;
    }
  },
  'bs'     => sub {  $OP = $_[0]; $DEVENV_REBUILD = 1; },
  'bsnr'   => sub {  $OP = 'bs';  $DEVENV_REBUILD = 0; },
  'bu'     => sub {  $OP = $_[0]; },
  'bwu'    => sub {  $OP = $_[0]; },
  'nv'     => sub {  $OP = $_[0]; },
  'fv'     => sub {  $OP = $_[0]; },
  'ud'     => sub {  $OP = $_[0]; },
  'upload' => sub {  $OP = $_[0]; },
);

sub build_client {
  my ($dest_client_wyc) = @_;
    
  say '--> wybuild -- Build wyUpdate.exe + client.wyc (/bwu)';
  do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bwu});

  if ($dest_client_wyc) {
    if (! -e 'wyUpdateProjects/wyUpdate/client.wyc') {
      say "!!! wyUpdateProjects/wyUpdate/client.wyc not generated";
      exit;
    }
    
    `mv wyUpdateProjects/wyUpdate/client.wyc $dest_client_wyc`;
    say "--- Saved: client.wyc, to: $dest_client_wyc";
  }
  else {
    say "--- Saved: client.wyc, to: wyUpdateProjects/wyUpdate/client.wyc";
  }
}

sub build_updates {
  say '--> wybuild -- Build all updates (/bu)';
  do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu}, {debug => 1});
}

sub build_new_version {
  my ($current_version, $next_version) = getCurrentAppVersion('intellaQueue');

  #say "($current_version, $next_version)"; exit;

  my $previous_deploy  = "$DEPLOYMENT_UPDATES/" . $current_version;
  my $new_deploy       = "$DEPLOYMENT_UPDATES/" . $next_version;
  my $addversion_extra = '';

  say "Build New Version: [$next_version] $new_deploy ?";
  my $build_new = <STDIN>;

  if ($build_new !~ /^y/) {
    say "Abort!";
    exit;
  }

  ###############################
  # Update the Build version info
  
  # Example /cygdrive/c/intellaApps/intellaQueue/src/intellaQueue/Properties/AssemblyInfo.cs
  my $assembly_info_filename = $APPS{intellaQueue}{gitBase} . '/intellaQueue/src/intellaQueue/Properties/AssemblyInfo.cs';
  my $assembly_info          = read_file($assembly_info_filename);
     $assembly_info =~ 'Version\("(.*)"\)';
  my $build_version          = BuildVersion($1);

  $assembly_info =~ s/Version\(".*"\)/Version("$build_version")/g;
  
  write_file($assembly_info_filename, $assembly_info);

  ##############################
      
  # NOTE: We MUST do a rebuild-all, otherwise Dlls and such will not be fully up to date in the source (intellaQueue/src/intellaQueue/bin/Debug)
  # NOTE: Otherwise we'll have a brand-new .exe and old .dlls being deployed
      
  say "--> Rebuild-All ($APPS{intellaQueue}{project})";
  build_application('/Rebuild');

  # We have a compiled app!
  
  `mkdir -p "$new_deploy" > /dev/null 2>&1`;
  
  # Clean up
  `rm -f wyUpdateProjects/Updates/*`;

  if ($next_version eq '1.0') {
    # No previous version!  Special case for 1.0.  Our very first version in wyUpdate is always 1.0 and 1.0 needs to be replaced/updated
    $addversion_extra = 'overwrite="true"';
  }
  else {
    # Copy existing files to speed up update (wyUpdate will not re-reploy unchanged files)
    say "--> Clone $current_version -> $next_version";
    say qq{cp -r "$previous_deploy" "$new_deploy"};
    `cp -rap "$previous_deploy"/* "$new_deploy"`;
    
    if ($? ne '0') {
      say "!!! Prepare Failed";
      exit;
    }
  }
  
  my $app_binary_stat       = File::stat::stat($APPS{intellaQueue}{appBinary});
  my $app_binary_mtime      = $app_binary_stat->mtime();
  my $app_binary_build_time = POSIX::strftime("%c %Z", localtime($app_binary_mtime));

  my $cwd = Cwd::getcwd();
  chdir($APPS{intellaQueue}{gitBase});
  my $git_version = `git rev-parse --verify HEAD`; chomp($git_version);
  chdir($cwd);

  $git_version .= " ($app_binary_build_time)";

  write_file("$new_deploy/version.txt",          $next_version);
  write_file("$new_deploy/version_build.txt",    $git_version);
  write_file("$new_deploy/default_settings.txt", $DEPLOYMENT->{default_server});

  say "--- Setup for wyUpdate: Previous: $previous_deploy -- New: $new_deploy";

  unlink(" intellaQueue/src/intellaQueue/bin/Debug/IntellaQueueRestarter.exe");
  
  # Anything that's new
  say "--> RSYNC New... -> $new_deploy";
  do_cmd(qq{rsync -rt intellaQueue/src/intellaQueue/bin/Debug/ "$new_deploy"/ $DEPLOYMENT_RSYNC_OPTS});

  # wyUpdate file
  my $wyupdate_xml = qq{<?xml version="1.0" encoding="utf-8"?>
<Versions>
    <AddVersion ${addversion_extra}>
        <Version>${next_version}</Version>
        <Changes>Updates</Changes>

        <InheritPrevRegistry />
        <InheritPrevActions />

        <Files dir="basedir">
            <Folder source="Z:\\Projects\\intellaQueue\\deploy\\${DEPLOYMENT_NAME}\\updates\\$next_version" insideonly="true" />
        </Files>
    </AddVersion>
</Versions>
  };

  #say $wyupdate_xml;

  say "--> Write add.xml";
  write_file('wyUpdateProjects/add.xml', $wyupdate_xml);
  
  # Build updates from 1.0 to our current version
  #build_updates(); # Generalize the below... pass params to build_updates
  say "--> wyUpdate -- Add/Build Updates...";

  # Build all versions to all other versions
  #my $out = do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu -vs="1.0"              -ve="$next_version" -add="add.xml"});

  my $opts = "";

  # If we're doing a first build... this is not needed
  if ($current_version ne '0.0') {
    $opts .= qq{/-vs="$current_version"};
  }

  # Only build from the current version to the next
  my $out = do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu $opts -ve="$next_version" -add="add.xml"});
  
  if ($out !~ /.*Succeeded.*Succeeded.*Succeeded.*/ms) {
    say "!!! wybuild.cmd.exe failed";
    say "";
    say qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu $opts -ve="$next_version" -add="add.xml"};
    say $out if (defined($out));
    exit;
  }

  build_client();
  `cp wyUpdateProjects/wyUpdate/client.wyc $new_deploy/`;
  say "--- Updated $new_deploy/client.wyc";

 #`rm $new_deploy/client.wyc`;
 # say "--- Clean $new_deploy/client.wyc";
  
  say "--> wyUpdate -- Add/Build Complete";

  say "--> wyUpdate -- Upload Updates...";
  $out = do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /upload});

  if ($? ne '0') {
    say "!!! wybuild.cmd.exe failed";
    say qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /upload};
    say $out if (defined($out));
    exit;
  }

  say "";
  say "*** DONE -- Built and Uploaded: $new_deploy";
  say "";

  return 1;
}

sub build_application {
  my ($build_op) = @_;
    
  `rm -f devenv.log`;
  my $cmd = qq{$CMD_CC $APPS{intellaQueue}{project} $build_op Debug /Project $APPS{intellaQueue}{setupProject} /ProjectConfig Debug /Out devenv.log};
  #say $cmd;
  do_cmd($cmd);
  if ($? ne '0') {
    say "!!! Build Failed.  Check devenv.log for more details";

    exit;
  }
}

sub fix_versions {
  my ($current_version, $next_version) = getCurrentAppVersion('intellaQueue');

  #say "($current_version, $next_version)"; exit;

  my $previous_deploy  = "$DEPLOYMENT_UPDATES/" . $current_version;
  my $addversion_extra = '';

  say "Fix all versions until: [$current_version] ?";
  my $foo = <STDIN>;

  $addversion_extra = 'overwrite="true"';

  my $cwd = Cwd::getcwd();
  chdir($DEPLOYMENT_UPDATES);
  my @previous_deploys = sort glob('*');
  chdir($cwd);

  say "Fixing...";
  print Dumper(\@previous_deploys);

  say "";

  foreach my $version (@previous_deploys) {
    # wyUpdate file
    my $wyupdate_xml = qq{<?xml version="1.0" encoding="utf-8"?>
<Versions>
    <AddVersion ${addversion_extra}>
        <Version>${version}</Version>
        <Changes>Updates</Changes>

        <InheritPrevRegistry />
        <InheritPrevActions />

        <Files dir="basedir">
            <Folder source="Z:\\Projects\\intellaQueue\\deploy\\${DEPLOYMENT_NAME}\\updates\\$version" insideonly="true" />
        </Files>
    </AddVersion>
</Versions>
  };

    say $wyupdate_xml;

    say "--> Write add.xml";
    write_file('wyUpdateProjects/add.xml', $wyupdate_xml);

    say "--> wyUpdate -- Add/Build Updates...";

    my $out = do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu -vs="1.0" -ve="$version" -add="add.xml"});
    if ($? ne '0') {
      say "!!! wybuild.cmd.exe failed";
      say "";
      say qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /bu -vs="1.0" -ve="$version" -add="add.xml"};
      say $out;
      exit;
    }
  }

  say "";
  say "DONE";
  say "";
}

sub BuildVersion {
  my ($previous_build) = @_;

  my $current_day = POSIX::strftime('%Y.%m.%d', localtime(time()));

  my $build_number = 0;
  
  if ($previous_build =~ /^$current_day\.(\d+)/) {
    $build_number = ($1 + 1);
  }
  
  return "${current_day}.$build_number";
}

sub do_cmd {
  my ($cmd, $opts) = @_;

  $opts          //= {};
  $opts->{debug} //= 0;

  unlink('/tmp/cmd_out');

  my $pid = fork();
  die if not defined $pid;

  if (!$pid) {
    # Child

    if ($opts->{debug})  {
      say "Exec: $cmd";
    }
    else {
      open(STDOUT,  '>', '/tmp/cmd_out') || die("Could not redirect STDOUT to /tmp/cmd_out");
    }

    say "Exec: $cmd"; # This goes to the out file

    exec($cmd);
    exit;
  }

  # parent
  waitpid($pid, 0);
  return read_file('/tmp/cmd_out') if (-e '/tmp/cmd_out');
}

if (!defined($DEPLOYMENT)) {
  say "Must specify deployment (-d)";
  exit;
}

say "";

if ($OP eq 'nv') {
  build_new_version();
  exit;
}

if ($OP eq 'fv') {
  fix_versions(); # Traverse every version in a deployment and update all the file include paths
  exit;
}

if ($OP eq 'bwu') {
  build_client();
  exit;
}

if ($OP eq 'bu') {
  build_updates();  
  exit;
}

#####################################################
# Build Setup/Installer (Bootstrapper + SetupProject)

if ($OP eq 'bs') {
  say "----------------------------------------------------------";
  say "--> Build Setup/Installer (Bootstrapper + SetupProject)";
  say "----------------------------------------------------------";
  say "";

  # Make sure the installer (SetupProject) has the latest version information so after we install we have the latest
  build_updates();
  build_client();
  `cp wyUpdateProjects/wyUpdate/* intellaQueue/src/SetupProject/`;
  say "--- Updated intellaQueue/src/SetupProject/client.wyc";

  say "";

  build_application();
  
  my $build_op;

  my ($current_version, $next_version) = getCurrentAppVersion('intellaQueue');
  my $current_deploy                  = "$DEPLOYMENT_UPDATES/" . $current_version;

  if (! -e "$current_deploy") {
    say "--! Creating Directory: $current_deploy";
    `mkdir -p "$current_deploy"`;
  }

  # Make sure we always have the latest client.wyc
  build_client("$current_deploy/client.wyc");

  # If this doesn't exist.. we're probably at version 1.0 but we've never built a version before
  # Example: $DEPLOYMENT_BASE_PATH/acs/updates/1.0/client.wyc

  if (! -e "$current_deploy") {
    say "!!! client.wyc not found: $current_deploy/client.wyc";
    exit;
  }

  if (! -e "$current_deploy/version.txt") {
    say "";
    say "!!! Version file not found: $current_deploy/version.txt";
    say "!!! Could not determine current application version.  Was an application built at all?  (Use -nv)";
    exit;
  }

  say "";

  say "--> Update SetupProject with latest deployment.  intellaQueue/src/intellaQueue/bin/Debug -> SetupProject/";
  do_cmd(qq{rsync -rt intellaQueue/src/intellaQueue/bin/Debug/ intellaQueue/src/SetupProject/bin/Debug $DEPLOYMENT_RSYNC_OPTS});
  `cp $current_deploy/client.wyc SetupProject/`; # We are deploying the current version when we build SetupProject

  write_file("SetupProject/default_settings.txt", $DEPLOYMENT->{default_server});

  say "";
  say "Start Build!";
  say "";

  #######

  say "--> Clean Projects...";

  `find intellaLib/ -name *.pdb -delete`;
  `find intellaLib/ -name *.dll -delete`;

  `find intellaQueue/src/intellaQueue -name *.pdb -delete`;
  #`find intellaQueue/src/intellaQueue -name *.dll -delete`;

  #######

  say "--> (Re)Build SetupProject...";
  if (-e 'SetupProject/bin/Debug/SetupProject.msi') {
    `mv SetupProject/bin/Debug/SetupProject.msi SetupProject/bin/Debug/SetupProject.msi.old`;
  }

  if ($DEVENV_REBUILD) {
    say "--- Setup for: Rebuild BootStrapper + SetupProject -- Deploy: intellaQueue-setup.exe";
    $build_op = '/Rebuild';
  }
  else {
    say "--- Setup for: Build BootStrapper + SetupProject -- Deploy: intellaQueue-setup.exe";
    $build_op = '/Build';
  }
  
  build_application($build_op);
  
  #######

  say "--> (Re)Build Bootstrapper...";
  if (-e 'intellaQueue/src/Bootstrapper/bin/Debug/intellaQueue-setup.exe') {
    `mv intellaQueue/src/Bootstrapper/bin/Debug/intellaQueue-setup.exe intellaQueue/src/Bootstrapper/bin/Debug/intellaQueue-setup.exe.old`;
  }

  `rm -f devenv.log`;
  my $cmd = qq{$CMD_CC $APPS{intellaQueue}{project} $build_op Debug /Project $APPS{intellaQueue}{bootstrapProject} /ProjectConfig Debug /Out devenv.log};
  #say $cmd;
  do_cmd($cmd);
  if (! -e 'intellaQueue/src/Bootstrapper/bin/Debug/intellaQueue-setup.exe') {
    say "!!! Build Failed.  Check devenv.log for more details";
    exit;
  }

  #######

  my $new_deploy = $DEPLOYMENT_INSTALLER;
  `mkdir -p "$new_deploy"`;

  say "--> Save installer: $new_deploy/intellaQueue-setup.exe";
  print `cp intellaQueue/src/Bootstrapper/bin/Debug/intellaQueue-setup.exe "$new_deploy"`;

  say "";
  say "DONE";
  say "";  

  exit;
}

if ($OP eq 'upload') {
  say "--> wyUpdate -- Upload Updates...";
  do_cmd(qq{$CMD_WYBUILD "$DEPLOYMENT_WYP" /upload});
}
