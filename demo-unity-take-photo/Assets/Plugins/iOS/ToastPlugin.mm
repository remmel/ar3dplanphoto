extern "C"{
#import "UIView+Toast.h"
#import "UnityAppController.h"
    
    void showToast(char *message, Boolean *isLong){
        NSString *messageFromUnity = [NSString stringWithUTF8String:message];
        UIView *displayView = [UIApplication sharedApplication].keyWindow.rootViewController.view;
        if(isLong){
            [displayView makeToast: messageFromUnity duration:3.5 position:CSToastPositionBottom ];
        } else {
            [displayView makeToast: messageFromUnity duration:2.0 position:CSToastPositionBottom ];
        }
    }
}

