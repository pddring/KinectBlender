bl_info = {
    "name": "Kinect MoCap",
    "category": "Object",
    "author": "Pete Dring",
    "version": (1,0),
}

import bpy
import socket
import re
import json
from bpy.props import (StringProperty, IntProperty, PointerProperty, BoolProperty)
from bpy.types import (PropertyGroup)

class KinectMoCapSettings(PropertyGroup):
    
    def enable_mo_cap(self, context):
        print("enabling")
        bpy.app.timers.register(self.trigger)
            
    def trigger(self):
        print("trigger")
        if self.enabled:
            #try:
                bpy.ops.scene.get_kinect_coordinates()
                return 1 / self.fps
            #except:
            #    self.enabled = False
        else:
            return None
            
    
    server: bpy.props.StringProperty(
        name="Server",
        default="http://localhost/armature.json",
        description="URL of Kinect web server"
    )
    
    fps: bpy.props.IntProperty(
        name = "FPS",
        description = "Frame rate",
        default = 1,
        min = 1,
        max = 30
    )
    
    enabled: bpy.props.BoolProperty(
        name = "Enabled",
        description = "Motion capture enabled",
        default = False,
        update = enable_mo_cap
    )

class KinectMoCapUI(bpy.types.Panel):
    bl_idname = "SCENE_PT_kinect"
    bl_label = "Kinect Motion Capture"
    bl_space_type = "PROPERTIES"
    bl_region_type = "WINDOW"
    bl_context = "scene"
    
    def draw(self, context):
        layout = self.layout
        scene = context.scene
        k = scene.kinect

        # Create a simple row.
        layout.label(text="Kinect server:")
        row = layout.row()
        row.prop(k, "server")
        

        row = layout.row()
        row.label(text="Client:")
        
        row = layout.row()
        row.operator("scene.get_kinect_coordinates")
        

        row.prop(k, "fps")
        row.prop(k, "enabled")
        

        

class KinectMoCap(bpy.types.Operator):
    bl_idname = "scene.get_kinect_coordinates"
    bl_label = "Get Kinect joint data"
    bl_options = {'REGISTER', 'UNDO'}

    
    def get_url(self, url):
        url = url.replace("http://", "")
        server = url.split("/")[0]
    
        args = url.replace(server,"")
        buffer = bytes()
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect((server, 80)) 

        s.send(bytes("GET /%s HTTP/1.0\r\nHost: %s\r\n\r\n" % (args, server), 'utf-8')) 
        byte_len = 0
        while 1:
            data = s.recv(1) #buffer
            if not data: break
        
            if data == b"\n":
                line = buffer.decode('utf-8')
                m = re.search("Content-Length: (\d+)", line)
                if m:
                    byte_len = int(m.group(1))
                
                buffer = bytes()
                if line.strip() == "":
                    buffer = s.recv(byte_len)
            
            else:
                buffer = buffer + data
            
        s.close()
        return buffer.decode('utf-8')

    
    
    def execute(self, context):
        k = context.scene.kinect
        
        joints = json.loads(self.get_url(k.server))
        
        print(joints)
    
        keys = bpy.context.scene.objects.keys()
        
        # add empties
        
        for j in joints["joints"]:
            if j['name'] in keys:
                print(j['name'], " already exists: moving")
                o = bpy.context.scene.objects[j['name']]
                o.location = (j['x'], j['y'], j['z'])
            else:
                print("creating", j['name'])
                o = bpy.data.objects.new(j['name'], None)
                bpy.context.scene.collection.objects.link(o)
                o.empty_display_size = .1
                o.empty_display_type = "PLAIN_AXES"
                o.location = (j['x'], j['y'], j['z'])
                o.show_name = True

        return {'FINISHED'}
    
def register():
    bpy.utils.register_class(KinectMoCapSettings)
    bpy.types.Scene.kinect = PointerProperty(type=KinectMoCapSettings)
    bpy.utils.register_class(KinectMoCap)
    bpy.utils.register_class(KinectMoCapUI)

def unregister():
    bpy.utils.unregister_class(KinectMoCap)
    bpy.utils.unregister_class(KinectMoCapUI)
    bpy.utils.unregister_class(KinectMoCapSettings)
    del bpy.types.Scene.kinect
    
if __name__ == "__main__":
    register()