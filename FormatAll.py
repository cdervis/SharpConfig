# Convenience script that formats all known source files in SharpConfig.
# Simply call this from the SharpConfig root folder, e.g.:
#
# > python FormatAll.py

import os

def formatSourcesIn(dir: str):
    for root, dirs, files in os.walk(dir):
        if 'bin' in root or 'obj' in root:
            continue

        print('-', root)
        for filename in files:
            if filename.endswith('.cs'):
                print('  -', filename)
                os.system(f'clang-format -i {os.path.join(root, filename)}')


print('Formatting all known source files...')

for folder in ['Src', 'Example', 'Tests']:
    formatSourcesIn(folder)

print('Formatting finished')
