import sys
from RDMInterface import RdmManager
def print_somthing(com_port): 
    rdmManager = RdmManager(com_port)
    data = rdmManager.get_firmware_data()
    for d in data:
        print(d) 
com_port = sys.argv[1]
print_somthing(com_port)
