// © Copyright 2016 HP Development Company, L.P.
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace More
{
    //
    // Syntax information originally taken from:
    //     http://httpd.apache.org/docs/current/configuring.html
    //
    // httpd configuration files contain one directive per line. The backslash "\" may be 
    // used as the last character on a line to indicate that the directive continues onto
    // the next line. There must be no other characters or white space between the backslash
    // and the end of the line.
    //
    // Arguments to directives are separated by whitespace. If an argument contains spaces,
    // you must enclose that argument in quotes.
    //
    // Directives in the configuration files are case-insensitive, but arguments to directives
    // are often case sensitive. Lines that begin with the hash character "#" are considered
    // comments, and are ignored. Comments may not be included on a line after a configuration
    // directive. Blank lines and white space occurring before a directive are ignored, so you
    // may indent directives for clarity.
    //
    // The values of variables defined with the Define of or shell environment variables can be
    // used in configuration file lines using the syntax ${VAR}. If "VAR" is the name of a valid
    // variable, the value of that variable is substituted into that spot in the configuration
    // file line, and processing continues as if that text were found directly in the
    // configuration file. Variables defined with Define take precedence over shell environment
    // variables. If the "VAR" variable is not found, the characters ${VAR} are left unchanged,
    // and a warning is logged. Variable names may not contain colon ":" characters, to avoid
    // clashes with RewriteMap's syntax.
    //
    // Only shell environment variables defined before the server is started can be used in
    // expansions. Environment variables defined in the configuration file itself, for example
    // with SetEnv, take effect too late to be used for expansions in the configuration file.
    //
    // The maximum length of a line in normal configuration files, after variable substitution
    // and joining any continued lines, is approximately 16 MiB. In .htaccess files, the maximum
    // length is 8190 characters.
    //
    // You can check your configuration files for syntax errors without starting the server by
    // using apachectl configtest or the -t command line option.
    //
    // You can use mod_info's -DDUMP_CONFIG to dump the configuration with all included files
    // and environment variables resolved and all comments and non-matching
    // <IfDefine> and <IfModule> sections removed. However, the output does not reflect the
    // merging or overriding that may happen for repeated directives.
    //
    //
    // Summary:
    //     1. One directive per line, backslash "\" used for line continuation
    //     2. Directive arguments separated by whitespace, use quotes for an argument that contains whitespace
    //     3. Use '#' character for comments
    //
}
