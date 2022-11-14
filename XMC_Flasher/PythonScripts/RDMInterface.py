import re
import math
from usbpro import UsbPro
from rdm import RdmPacket
import time
from multiprocessing import Event
#define E120_SOFTWARE_VERSION_LABEL                       0x00C0
#define E120_BOOT_SOFTWARE_VERSION_ID                     0x00C1
#define E120_DEVICE_LABEL                                 0x0082
#define E120_DEVICE_INFO                                  0x0060

BLUE = '4c 42 56 46 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 CC 00 FF 00 00'
GREEN = '4c 42 56 46 00 00 00 00 00 00 00 00 00 00 00 00 00 CC 00 FF 00 00 00 00 00 00 00 00'
RED = '4c 42 56 46 00 00 00 00 00 00 00 CC 00 FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00'
OFF = '4c 42 56 46 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00'
ON = '255 255 0 0 0 0'
White = ''
HEADERS = {'content-type': 'application/x-www-form-urlencoded'} 
DISCOVER_COMMAND = '10'
GET_COMMAND = '20'
SET_COMMAND = '30'
TIME_OUT = 5
WATTAGE_PID = '8626'
SOFTWARE_VERSION = '00C0'
DEVICE_INFO = '0060'
INPUT_PID = '1000'
DISC_UNIQUE_BRANCH = '0001'
DISC_MUTE = '0002'
DISC_UN_MUTE = '0003'
DISCOVERY_COMMAND = '10'
STATUS_ACKNOWLEDGE = 'ack'
STATUS_NOTACKNOWLEDGE = 'nack'
STATUS_OTHER = 'other'
STATUS_NONE = 'none'
CLEANER_RE = re.compile('[^0-9A-Za-z]') 

class RdmManager:
    def __init__(self, usbDevice):
        super().__init__()   
        global transaction
        transaction = 1
        self.__rdmUsbDevice = usbDevice
        self.isInitialized = False
        self.errorMessage = None
        self.__intializeUsbPro()       

    def __intializeUsbPro(self):
        try:
            self.__usbPro = UsbPro(self.__rdmUsbDevice)
            self.isInitialized = True
        except Exception as ex:
            print("Failed to initialize")        
            #self.errorMessage = f'There is an issue initializing the DMX Device. {ex}'
            #print(f'There is an issue initializing the DMX Device. {ex}')

    def doTest(self, value):
        color = GREEN
        if((value % 1) == 0):
            color = RED
        isSuccessful = False
        attempts = 0    
        self.sendRDM("FFFFFFFFFFFF", color)
        return    
        while(isSuccessful == False or attempts < 3):
            r = self.sendRDM("FFFFFFFFFFFF", color)
            isSuccessful = (self.__rdm_response_type(r) == STATUS_ACKNOWLEDGE)
            attempts += 1
     
    def turnLEDOff(self, target):
        isSuccessful = False
        attempts = 0        
        while(isSuccessful == False or attempts < 3): 
            r = self.sendRDM(target, OFF)
            isSuccessful = (self.__rdm_response_type(r) == STATUS_ACKNOWLEDGE)
            attempts += 1
        
    
    def turnLEDOnForDiscovery(self): 
        self.broadCastDMX(ON)
    
    def broadCastRDM(self, color):
        self.sendRDM("FFFFFFFFFFFF", color)
    
    def broadCastRDMLEDOff(self):
        self.sendRDM("FFFFFFFFFFFF", OFF)

    def sendRDM(self, target, color):
        global transaction
        p = RdmPacket()
        p.destination_uid = self.__to_bytes(target)
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes('30')[0]
        p.pid = self.__to_bytes('8625')
        p.data = self.__to_bytes(color)        
        return self.__usbPro.rdm(p.serialize()) 

    def resetRDM(self):
        self.sendRDM("FFFFFFFFFFFF", "")
        time.sleep(0.003)

    def resetDMX(self):
        self.broadCastDMX("0 0 0 0 0 0")
        time.sleep(0.003)

    def dmx(self, data):
        dmx = [0x00]*513
        for k, v in data:
            if k.isnumeric():
                dmx[int(k)] = int(v)

        self.__usbPro.dmx(bytes(dmx))
        return '{"status":"ok"}'

    def broadCastDMX(self, data): 
        dmx = [0x00]*513 
        values = data.split(' ')
        for i, v in enumerate(values):
            dmx[i+1] = int(v)  
        self.__usbPro.dmx(bytes(dmx))

    def get_firmware_data(self):
        signal = Event()
        address = self.discover_all_devices(signal)        
        if (address is None and len(address) < 1):
            return (1, "", "", "")

        address = address[0].split(' ') 
        address.insert(2,':')
        address = ''.join(address)
        software_version_command = {'destination' : address, 'command_class' : GET_COMMAND, 'pid' : SOFTWARE_VERSION, 'data' : ''}        
        data = self.rdm(software_version_command)   
        if (data is None and len(data) < 1):
            return (2, "", "", "")
        software_version = ''.join(map(chr, data[data[1:-1].index(192) + 2:-1])) 
        #wattage_command = {'destination' : address, 'command_class' : GET_COMMAND, 'pid' : WATTAGE_PID, 'data' : ''}    
        #data = self.rdm(wattage_command)   
        #if (data is None and len(data) < 1):
        #    return (3, "", "", "")
        wattage = ""  
        return (0, address, software_version, wattage)

    def rdm(self, data):
        global transaction
        p = RdmPacket() 
        p.destination_uid = self.__to_bytes(data['destination'])
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes(data['command_class'])[0]
        p.pid = self.__to_bytes(data['pid'])
        p.data = self.__to_bytes(data['data'])
        
        r = self.__usbPro.rdm(p.serialize())

        if r.type == 12:
            r = self.__usbPro.rdm(p.serialize()) 
        return r.data[1:]

    def test_rdm_discovery(self):
        self.rdm_discovery('000000000000', 'FFFFFFFFFFFF')

    def rdm_discovery(self, low, high): 
        global transaction
        p = RdmPacket()
        p.destination_uid = bytes([0xFF,0xFF,0xFF,0xFF,0xFF,0xFF])
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes("10")[0] # 0x10 = E120_DISCOVERY_COMMAND
        p.pid = self.__to_bytes("00 01") # 0x0001 == E120_DISC_UNIQUE_BRANCH
        p.data = self.__to_bytes(low)+self.__to_bytes(high)
        
        r = self.__usbPro.rdm_disc_unique(p.serialize())
         
        return r.data[1:]
    
    def discover_all_devices(self, signal):
        self.__unmute_all_devices()
        lowTarget = 0x000000000000
        highTarget = 0xFFFFFFFFFFFF
        addresses = None
        checkForMoreAddresses = True
        while(checkForMoreAddresses):
            _addresses = self.__get_devices(lowTarget, highTarget, signal) 
            if(_addresses != None):
                checkForMoreAddresses = True
                if(addresses == None):
                    addresses = []
                addresses.extend(_addresses)
            else:
                checkForMoreAddresses = False    
        return addresses            

    def __get_devices(self, lowTarget, highTarget, signal):  
        addresses = None 
        if(signal.is_set()):
            return addresses
        if(lowTarget == highTarget):
            #print(f'Same addresses so skip discovery and mute device {lowTarget}')
            deviceAddress = '{:012x}'.format(lowTarget)
            if(self.__mute_device(deviceAddress)):
                formattedAddress = ' '.join(a+b for a,b in zip(deviceAddress[::2], deviceAddress[1::2]))
                addresses = [formattedAddress]
            else:
                print("Failed to mute target")
                #print('Failed to mute device: {lowTarget}')
        else:
            hasMultipleDevices, addresses = self.__attempt_to_get_device(lowTarget, highTarget, signal)
            if(hasMultipleDevices):
                middleTarget = math.floor((lowTarget + highTarget)/2) 
                lowAddresses = self.__get_devices(lowTarget, middleTarget, signal) 
                if(lowAddresses):
                    if(not addresses):
                        addresses = []
                    addresses.extend(lowAddresses) 
                highAddresses = self.__get_devices(middleTarget + 1, highTarget, signal)         
                if(highAddresses):
                    if(not addresses):
                        addresses = []
                    addresses.extend(highAddresses)
        return addresses

    def __attempt_to_get_device(self, lowTarget, highTarget, signal): 
        addresses = None
        if(signal.is_set()):
            return addresses
        
        global transaction
        p = RdmPacket()
        p.destination_uid = bytes([0xFF,0xFF,0xFF,0xFF,0xFF,0xFF])
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes(DISCOVERY_COMMAND)[0] # 0x10 = E120_DISCOVERY_COMMAND
        p.pid = self.__to_bytes(DISC_UNIQUE_BRANCH) 
        p.data = self.__to_bytes('{:012x}'.format(lowTarget)) + self.__to_bytes('{:012x}'.format(highTarget))      
        #p.data = self.__to_bytes('000000000000')+self.__to_bytes('FFFFFFFFFFFF')  
        attempts = 0
        hasMultipleDevices = False
        while(not addresses and attempts < 2):
            hasMultipleDevices = False          
            r = self.__usbPro.send_discovery_command(p.serialize())
            if(r and r.data and len(r.data) > 0):
                if(self.__is_valid_discovery_checksum(r.data)):
                    deviceAddress = ' '.join('{:02x}'.format(x) for x in self.__look_for_discovery_response(r.data[1:])) 
                    if(self.__mute_device(deviceAddress)):
                        addresses = [deviceAddress]
                else:
                    hasMultipleDevices = True
                    #print(f'Multiple devices were found for addresses range {lowTarget} to {highTarget}')
                    break
            attempts += 1 
        return (hasMultipleDevices, addresses)

    def __unmute_all_devices(self): 
        global transaction
        p = RdmPacket()
        p.destination_uid = bytes([0xFF,0xFF,0xFF,0xFF,0xFF,0xFF])
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes(DISCOVERY_COMMAND)[0]
        p.pid = self.__to_bytes(DISC_UN_MUTE) 
        self.__usbPro.send_discovery_command(p.serialize())          
        #print(f'unmuting all devices') 

    def __mute_device(self, targetAddress): 
        global transaction
        p = RdmPacket() 
        p.destination_uid = self.__to_bytes(targetAddress)
        sn = self.__usbPro.serial_number
        p.source_uid = bytes([0x45,0x4e,sn[3],sn[2],sn[1],sn[0]])
        p.transaction_number = transaction
        transaction = (transaction + 1) % 255
        p.port_id_or_response_type = 1
        p.command_class = self.__to_bytes(DISCOVERY_COMMAND)[0]   
        p.pid = self.__to_bytes(DISC_MUTE) 
        attemps = 0
        isSuccessful = False
        while(isSuccessful == False and attemps < 3):
            r = self.__usbPro.send_discovery_command(p.serialize()) 
            isSuccessful = (self.__rdm_response_type(r) == STATUS_ACKNOWLEDGE)
            attemps += 1      
        #print(f'Muting target {targetAddress}: {isSuccessful}')
        return isSuccessful

    def __is_valid_checksum(self, data):
        if(data is None): 
            return False
        slotTotal = sum(data[: -2])
        checksumHexString = ''.join('{:02x}'.format(x) for x in data[-2: -1])
        checksumTotal = int(checksumHexString, 16) 
        return (slotTotal == checksumTotal)
    
    def __is_valid_discovery_checksum(self, data):
        if(data is None): 
            return False
        i = next((i for i, d in enumerate(data) if(d == 0xaa)), None)
        if(i is None): 
            return False 
        slotTotal = sum(data[i + 1: - 4])
        checksumHexString = '{:02x}{:02x}'.format((data[-4] & data[-3]), (data[-2] & data[-1]))
        checksumTotal = int(checksumHexString, 16) 
        return (slotTotal == checksumTotal)

    def __look_for_discovery_response(self, data):
        if len(data) == 0:
            return ''
        for i in range(0, len(data)):
            if data[i] == 0xaa:
                break
        i+=1
        
        out = []
        for j in range(0, 6):
            if len(data) < i + 2:
                return ''
            a = data[i]
            b = data[i+1]
            v = (a & ~0xAA) + (b & ~0x55)
            i+=2
            out.append(v)
    
        return bytes(out)
    

    def __to_bytes(self, s):
        if s == None or len(s) == 0:
            return bytes([])
        #print(s)
        cleaned = CLEANER_RE.sub('',s)
        #print(cleaned)
        b = bytes.fromhex(cleaned)
        #print(' '.join('{:02x}'.format(x) for x in b))
        return b

    def __rdm_response_type(self, r):
        MESSAGE_LENGTH_INDEX = 3
        if r is None or r.data is None or len(r.data) < MESSAGE_LENGTH_INDEX:
            return STATUS_NONE
        statusIndex = r.data[MESSAGE_LENGTH_INDEX] 
        if len(r.data) <= statusIndex:
            return STATUS_NONE
        code = r.data[statusIndex]
        if code == 0x00:
            return STATUS_ACKNOWLEDGE
        if code == 0x02:
            return STATUS_NOTACKNOWLEDGE
        else:
            return STATUS_OTHER

