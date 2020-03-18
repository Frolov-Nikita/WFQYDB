#!/usr/bin/python3
import netifaces
import json
from pathlib import Path

default_gateway = netifaces.gateways()['default'][netifaces.AF_INET][0]
path = Path('Cfg.json')
data = json.loads(path.read_text())
data["WFQYDBConnection"] = "TcpClientStreamSource; "+default_gateway+":6789"  # replace list
path.write_text(json.dumps(data,indent=4, separators=(',', ': ')), encoding='utf-8')
