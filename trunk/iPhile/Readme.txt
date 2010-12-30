Copyright (c) 2010 ebbes <ebbes.ebbes@gmail.com>
All rights reserved.

This file is part of iPhile.

iPhile is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

iPhile is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with iPhile.  If not, see <http://www.gnu.org/licenses/>.


iPhile is a simple tool that maps any iOS device's filesystem into Windows Explorer.
It uses Manzana.dll and Dokan (and its .NET binding).
Install Dokan version 0.5.3 or higher before trying to use this tool.
 
BEWARE: This software has development quality!
I give absolutely NO WARRANTY for your device. Only use this software if you REALLY know what you are doing.
It is all your fault if this software bricks your iPhone or iPod touch or iPad or your computer or if it
explodes, becomes slow or anything else.
You have been warned. (Please see license in file "COPYING.txt")

Planned features:
 - Maybe a GUI.
 - Maybe use Thumbnails for .app folders. (Could be done by emulating a desktop.ini?)
 - Maybe converters for PNG and PLIST files

--------------------------------------------------------------------------------------

DriveUnmounter

DriveUnmounter is an additional tool bundled with iPhile. It is able to unmount Dokan drives
(but also other removable drives!)
Use this tool if one iPhone drive got stuck in Windows Explorer.
This means it is removed but still shown in Windows Explorer.
This can happen if iPhile crashes.

BEWARE: Do ONLY use this tool for drives created by iPhile that were not properly unmounted!
BEWARE: Do NOT use this tool for drives not created by iPhile!
BEWARE: USE THIS TOOL AT YOUR OWN RISK!
BEWARE: Removing any removable drive other than unproperly dismounted iDevices may result in loss of data!
You have been warned. (Please see license below.)

--------------------------------------------------------------------------------------
Software Licenses

DokanNet Binding using Dokan, both available at http://dokan-dev.net/:

 Copyright (C) 2010 ebbes <ebbes.ebbes@gmail.com>                            
                                                                             
 This program is free software; you can redistribute it and/or modify it     
 under the terms of the GNU General Public License as published by the Free  
 Software Foundation; either version 3 of the License, or (at your option)   
 any later version.                                                          
                                                                             
 This program is distributed in the hope that it will be useful, but WITHOUT 
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       
 FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    
 more details.                                                               
 You should have received a copy of the GNU General Public License along     
 with this program; if not, see <http://www.gnu.org/licenses>.               

 This file is based on work under the following copyright and permission
 notice:

  Copyright (c) 2007 Hiroki Asakawa

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
  
Manzana.dll taken out of iPhoneFS project available at http://code.google.com/p/iphonefs/

 Copyright (C) 2010 ebbes <ebbes.ebbes@gmail.com>                            
                                                                             
 This program is free software; you can redistribute it and/or modify it     
 under the terms of the GNU General Public License as published by the Free  
 Software Foundation; either version 3 of the License, or (at your option)   
 any later version.                                                          
                                                                             
 This program is distributed in the hope that it will be useful, but WITHOUT 
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       
 FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    
 more details.                                                               
 You should have received a copy of the GNU General Public License along     
 with this program; if not, see <http://www.gnu.org/licenses>.               

 This file is based on work under the following copyright and permission
 notice:


  Copyright (C) 2007-2011 Lokkju, Inc <lokkju@lokkju.com>                     
                                                                              
  This program is free software; you can redistribute it and/or modify it     
  under the terms of the GNU General Public License as published by the Free  
  Software Foundation; either version 3 of the License, or (at your option)   
  any later version.                                                          
                                                                              
  This program is distributed in the hope that it will be useful, but WITHOUT 
  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or       
  FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for    
  more details.                                                               
  You should have received a copy of the GNU General Public License along     
  with this program; if not, see <http:www.gnu.org/licenses>.               
                                                                              
  Additional permission under GNU GPL version 3 section 7:                    
  If you modify this Program, or any covered work, by linking or combining it 
  with the NeoGeo SMB library, or a modified version of that library,         
  the licensors of this Program grant you additional permission to convey the 
  resulting work as long as the library is distributed without fee.           


   This file is based on work under the following copyright and permission
   notice:
   Software License Agreement (BSD License)
   
   Copyright (c) 2010, Lokkju Inc. <lokkju@lokkju.com>
   Copyright (c) 2007, Peter Dennis Bartok <PeterDennisBartok@gmail.com>
   All rights reserved.
   
   Redistribution and use of this software in source and binary forms, with or without modification, are
   permitted provided that the following conditions are met:
   
   Redistributions of source code must retain the above
     copyright notice, this list of conditions and the
     following disclaimer.
   
   Redistributions in binary form must reproduce the above
     copyright notice, this list of conditions and the
     following disclaimer in the documentation and/or other
     materials provided with the distribution.
   
   Neither the name of Peter Dennis Bartok nor the names of its
     contributors may be used to endorse or promote products
     derived from this software without specific prior
     written permission of Yahoo! Inc.
   
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
   WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
   PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
   ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
   LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
   INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
   TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
   ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
   
iPhile and DriveUnmounter

	(See "COPYING.txt")