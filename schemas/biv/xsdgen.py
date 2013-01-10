

import os
import subprocess

xsds = [d for d in os.listdir('.') if d.endswith('.xsd')]

args = [r'\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\xsd.exe']

for xsd in xsds:
    args.append(xsd)

args[-1] = ".\\" + args[-1]

args.extend(['/c', '/n:eDAIS'])

subprocess.call(args, shell=True)
