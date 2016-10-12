#include <OpenGLES/ES3/gl.h>
#include <OpenGLES/ES3/glext.h>


extern UIViewController *UnityGetGLViewController(); // Root view controller of Unity screen.
extern void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnityPause( bool pause ) {
    
}




@interface NSDictionary (BVJSONString)
-(NSString*) bv_jsonStringWithPrettyPrint:(BOOL) prettyPrint;
@end
@implementation NSDictionary (BVJSONString)
-(NSString*) bv_jsonStringWithPrettyPrint:(BOOL) prettyPrint {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:self
                                                       options:(NSJSONWritingOptions)    (prettyPrint ? NSJSONWritingPrettyPrinted : 0)
                                                            error:&error];
    
    if (! jsonData) {
        NSLog(@"bv_jsonStringWithPrettyPrint: error: %@", error.localizedDescription);
        return @"{}";
    } else {
        return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    }
}
@end



#pragma mark Plug-in Functions


@interface PickerDelegate : UIViewController<UIImagePickerControllerDelegate, UINavigationControllerDelegate> {
    char fileUriStr[1024];
    int callCount;
};
-(id)init;
-(void)dealloc;
-(void)imagePickerController:(UIImagePickerController*)picker
       didFinishPickingImage:(UIImage*)image editingInfo:(NSDictionary*)info;
+(char*)makeStrCopy:(char*) str;
-(char*)getFileUriStr;
-(int)getCallCount;
@property (nonatomic,retain) UIPopoverController* controller;
@property (nonatomic,retain) NSString* callbackName;
@property (nonatomic,retain) UIImagePickerController* imagePicker;
@property( nonatomic,retain) NSString* lastImageUri;
@property( nonatomic,assign) BOOL transformOnImport;
@property (nonatomic,assign) int importedOrientation;
@end

static PickerDelegate* pickDele = NULL;


@implementation PickerDelegate;
@synthesize controller, imagePicker, lastImageUri;
@synthesize transformOnImport;
@synthesize importedOrientation;


+(void) load {
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(appDidFinishLaunching:)
                                                 name:UIApplicationDidFinishLaunchingNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(appDidBecomeActive:)
                                                 name:UIApplicationDidBecomeActiveNotification object:nil];
}


+(void) appDidFinishLaunching:(id)ntf {
    if(pickDele==NULL) pickDele = [[[PickerDelegate alloc] init] retain];
}


+(void) appDidBecomeActive:(id)ntf {
    if(pickDele==NULL) pickDele = [[[PickerDelegate alloc] init] retain];
}


-(UIImagePickerController*) getImagePickerController {
    if( self.imagePicker == nil ) {
        UIImagePickerController * ip = [[UIImagePickerController alloc] init];
        self.imagePicker = ip;
        self.imagePicker.sourceType = UIImagePickerControllerSourceTypeCamera;
        self.imagePicker.delegate = self;
    }
    return self.imagePicker;
}


-(id) init {
    callCount = 0;
    self.transformOnImport = YES;
    self.importedOrientation = 0;
    [self getImagePickerController];
    return [super init];
}


-(void)dealloc{
    self.callbackName = nil;
    self.controller = nil;
    self.imagePicker = nil;
    [super dealloc]; 
}


-(void)imagePickerController:(UIImagePickerController*)picker didFinishPickingImage:(UIImage*)image editingInfo:(NSDictionary*)info {
    callCount++;
    
    if( self.transformOnImport == YES ) {
        image = [self scaleAndRotateImage:image];
    }
    self.importedOrientation = [self getEXIFOrientation:image];
    
    NSData *imageData = UIImageJPEGRepresentation(image,0.6);
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *path=[[paths objectAtIndex:0] stringByAppendingPathComponent:@"temp.jpg"];
    NSString *fileUri=[NSString stringWithFormat:@"file:%@?ori=%d&cnt=%d",path,image.imageOrientation,callCount];
    
    strcpy( fileUriStr, (char*)[fileUri UTF8String] );
    [imageData writeToFile:path atomically:YES];
    self.lastImageUri = fileUri;
    [self.imagePicker.presentingViewController dismissModalViewControllerAnimated:YES];
    [self performSelector:@selector(okCallback) withObject:nil afterDelay:0.125f];
}


- (void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {}
    [self.imagePicker.presentingViewController dismissModalViewControllerAnimated:YES];
    [self performSelector:@selector(cancelCallback) withObject:nil afterDelay:0.125f];
}


-(void) okCallback {
    UnityPause( false );
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
    } else {
    }
    if( self.callbackName  != nil ) {
        NSDictionary * dictRet = [[NSDictionary alloc] initWithObjectsAndKeys:
                              lastImageUri, @"path", @"true", @"succeeded", @"false", @"cancelled", nil];
        NSString* strRet = [dictRet bv_jsonStringWithPrettyPrint:TRUE];
        UnitySendMessage( [self.callbackName UTF8String], "CallDelegateFromNative", [strRet UTF8String] );
    }
    self.callbackName = nil;
}


-(void) cancelCallback {
    UnityPause( false );
    if( self.callbackName  != nil ) {
        NSDictionary * dictRet = [[NSDictionary alloc] initWithObjectsAndKeys:
                                  @"", @"path", @"false", @"succeeded", @"true", @"cancelled", nil];
        
        NSString* strRet = [dictRet bv_jsonStringWithPrettyPrint:TRUE];
        UnitySendMessage( [self.callbackName UTF8String], "CallDelegateFromNative", [strRet UTF8String] );
    }
    self.callbackName = nil;
}



+(char*)makeStrCopy:(char*)str{ 
    return (str==NULL) ? NULL : strcpy((char*)malloc(strlen(str)+1), str); 
}


-(char*)getFileUriStr{
    return [PickerDelegate makeStrCopy:fileUriStr]; 
}


-(int)getCallCount{ 
    return callCount; 
}


- (UIImage*) scaleAndRotateImage:(UIImage *)image {
    
    // Support maximum texture size of 2048 of larger devices.
    static GLint kMaxResolution = 2048;
    static bool kMaxResolutionResolved = false;
    if( !kMaxResolutionResolved ) {
        kMaxResolutionResolved=true;
        glGetIntegerv( GL_MAX_TEXTURE_SIZE, &kMaxResolution);
        if( kMaxResolution < 1024 ) {  // Floor to 1024 and ceil to 2048
            kMaxResolution = 1024;
        } else if( kMaxResolution > 2048 ) {
            kMaxResolution = 2048;
        }
        // Make sure that nothing wierd is returned.
        if( kMaxResolution != 1024 && kMaxResolution != 2048 ) {
            kMaxResolution = 1024;
        }
    }
    
    CGImageRef imgRef = image.CGImage;
    
    CGFloat width = CGImageGetWidth(imgRef);
    CGFloat height = CGImageGetHeight(imgRef);
    
    
    CGAffineTransform transform = CGAffineTransformIdentity;
    CGRect bounds = CGRectMake(0, 0, width, height);
    if (width > kMaxResolution || height > kMaxResolution) {
        CGFloat ratio = width/height;
        if (ratio > 1) {
            bounds.size.width = kMaxResolution;
            bounds.size.height = roundf(bounds.size.width / ratio);
        }
        else {
            bounds.size.height = kMaxResolution;
            bounds.size.width = roundf(bounds.size.height * ratio);
        }
    }
    
    CGFloat scaleRatio = bounds.size.width / width;
    CGSize imageSize = CGSizeMake(CGImageGetWidth(imgRef), CGImageGetHeight(imgRef));
    CGFloat boundHeight;
    UIImageOrientation orient = image.imageOrientation;
    switch(orient) {
            
        case UIImageOrientationUp: //EXIF = 1
            transform = CGAffineTransformIdentity;
            break;
            
        case UIImageOrientationUpMirrored: //EXIF = 2
            transform = CGAffineTransformMakeTranslation(imageSize.width, 0.0);
            transform = CGAffineTransformScale(transform, -1.0, 1.0);
            break;
            
        case UIImageOrientationDown: //EXIF = 3
            transform = CGAffineTransformMakeTranslation(imageSize.width, imageSize.height);
            transform = CGAffineTransformRotate(transform, M_PI);
            break;
            
        case UIImageOrientationDownMirrored: //EXIF = 4
            transform = CGAffineTransformMakeTranslation(0.0, imageSize.height);
            transform = CGAffineTransformScale(transform, 1.0, -1.0);
            break;
            
        case UIImageOrientationLeftMirrored: //EXIF = 5
            boundHeight = bounds.size.height;
            bounds.size.height = bounds.size.width;
            bounds.size.width = boundHeight;
            transform = CGAffineTransformMakeTranslation(imageSize.height, imageSize.width);
            transform = CGAffineTransformScale(transform, -1.0, 1.0);
            transform = CGAffineTransformRotate(transform, 3.0 * M_PI / 2.0);
            break;
            
        case UIImageOrientationLeft: //EXIF = 6
            boundHeight = bounds.size.height;
            bounds.size.height = bounds.size.width;
            bounds.size.width = boundHeight;
            transform = CGAffineTransformMakeTranslation(0.0, imageSize.width);
            transform = CGAffineTransformRotate(transform, 3.0 * M_PI / 2.0);
            break;
            
        case UIImageOrientationRightMirrored: //EXIF = 7
            boundHeight = bounds.size.height;
            bounds.size.height = bounds.size.width;
            bounds.size.width = boundHeight;
            transform = CGAffineTransformMakeScale(-1.0, 1.0);
            transform = CGAffineTransformRotate(transform, M_PI / 2.0);
            break;
            
        case UIImageOrientationRight: //EXIF = 8
            boundHeight = bounds.size.height;
            bounds.size.height = bounds.size.width;
            bounds.size.width = boundHeight;
            transform = CGAffineTransformMakeTranslation(imageSize.height, 0.0);
            transform = CGAffineTransformRotate(transform, M_PI / 2.0);
            break;
            
        default:
            [NSException raise:NSInternalInconsistencyException format:@"Invalid image orientation"];
            
    }
    
    UIGraphicsBeginImageContext(bounds.size);
    
    CGContextRef context = UIGraphicsGetCurrentContext();
    
    if (orient == UIImageOrientationRight || orient == UIImageOrientationLeft) {
        CGContextScaleCTM(context, -scaleRatio, scaleRatio);
        CGContextTranslateCTM(context, -height, 0);
    }
    else {
        CGContextScaleCTM(context, scaleRatio, -scaleRatio);
        CGContextTranslateCTM(context, 0, -height);
    }
    
    CGContextConcatCTM(context, transform);
    
    CGContextDrawImage(UIGraphicsGetCurrentContext(), CGRectMake(0, 0, width, height), imgRef);
    UIImage *imageCopy = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    return imageCopy;
}


- (int)getEXIFOrientation:(UIImage *)image {
    UIImageOrientation orient = image.imageOrientation;
    switch(orient) {
        case UIImageOrientationUp: return 1; // ORIENTATION_NORMAL == 1
        case UIImageOrientationUpMirrored:  return 2; // ORIENTATION_FLIP_HORIZONTAL = 2
        case UIImageOrientationDown: return 3; // ORIENTATION_ROTATE_180 = 3
        case UIImageOrientationDownMirrored: return 4; // ORIENTATION_FLIP_VERTICAL = 4
        case UIImageOrientationLeftMirrored: return 5; // ORIENTATION_TRANSPOSE = 5
        case UIImageOrientationLeft: return 6; // ORIENTATION_ROTATE_90 = 6
        case UIImageOrientationRightMirrored: return 7; // ORIENTATION_TRANSVERSE = 7
        case UIImageOrientationRight: return 7; // ORIENTATION_ROTATE_270 = 8
    }
    return 0; // ORIENTATION_UNDEFINED
}




@end



extern "C" char* _ImagePickerOpen ( int type, bool frontFacingIsDefault, BOOL _transformOnImport, char* callbackName ) {
    if(pickDele==NULL) pickDele = [[PickerDelegate alloc] init];
    
    UIImagePickerController* imagePicker = [pickDele getImagePickerController];

    pickDele.transformOnImport = _transformOnImport;
    
    if( type == 0 ) {
        imagePicker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
    } else if( type == 1 ) {
        imagePicker.sourceType = UIImagePickerControllerSourceTypeCamera; // hopefully the camera is preheated and ready to display
        if( frontFacingIsDefault ) {
            if( [UIImagePickerController isCameraDeviceAvailable: UIImagePickerControllerCameraDeviceFront] ) {
                imagePicker.cameraDevice = UIImagePickerControllerCameraDeviceFront;
            }
        }
    } else if( type == 2 ) {
        if ( [UIImagePickerController isSourceTypeAvailable:UIImagePickerControllerSourceTypeSavedPhotosAlbum] ) {
            imagePicker.sourceType = UIImagePickerControllerSourceTypeSavedPhotosAlbum;
        } else {
            imagePicker.sourceType = UIImagePickerControllerSourceTypePhotoLibrary;
        }
    }
    
    UnityPause( true );
    
    pickDele.callbackName = [NSString stringWithFormat:@"%s",callbackName];
   
    id rootVC = [[[[[UIApplication sharedApplication] keyWindow] subviews] objectAtIndex:0] nextResponder];
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPad) {
        
        if( type == 0 || type == 2 ) {
            UIPopoverController *popover = [[[UIPopoverController alloc] initWithContentViewController:imagePicker] autorelease];
            if ( UnityGetGLViewController().view.window != nil ) {
                [popover presentPopoverFromRect:CGRectMake(44, 30, 111, 111) inView:UnityGetGLViewController().view permittedArrowDirections:UIPopoverArrowDirectionAny animated:YES];
            } else {
                
                [popover presentPopoverFromRect:[UIScreen mainScreen].bounds
                                         inView:UnityGetGLViewController().view
                       permittedArrowDirections:UIPopoverArrowDirectionAny
                                       animated:YES];
            }
            pickDele.controller = popover;
        } else {
            [rootVC presentViewController:imagePicker animated:YES completion:^{
                //do nothing
            }];
        }
    }
    else{
        [rootVC presentModalViewController:imagePicker animated:YES];
    }
    
    return (pickDele!=NULL) ? [pickDele getFileUriStr] : NULL;
}


extern "C" int _ImagePickerGetLastImportedOrientation() {
    if( pickDele == nil ) return 0;
    return pickDele.importedOrientation;
}


extern "C" char* _ImagePickerGetPath () {
    return (pickDele!=NULL) ? [pickDele getFileUriStr] : NULL;
}


extern "C" int _ImagePickerGetCallCount () {
    return (pickDele!=NULL) ? [pickDele getCallCount] : 0;
}


extern "C" bool _CheckCameraAvailable() {
    if ([UIImagePickerController isSourceTypeAvailable: UIImagePickerControllerSourceTypeCamera]) {
        return true;
    }
    return false;
}


extern "C" const char* ImagePicker_GetDocumentDirectory() {
    NSArray* paths = NSSearchPathForDirectoriesInDomains( NSDocumentDirectory, NSUserDomainMask, YES );
    NSArray* paths2 = NSSearchPathForDirectoriesInDomains( NSDocumentDirectory, NSAllDomainsMask, YES );
    
    NSString* basePath = @"";
    if( [paths count] > 0 ) {
        basePath = [NSString stringWithFormat:@"%@", [paths objectAtIndex:0] ];
    }
    
    const char* str = [basePath UTF8String];
    const char* ret = strcpy((char*)malloc(strlen(str)+1), str);
    return ret;
}



