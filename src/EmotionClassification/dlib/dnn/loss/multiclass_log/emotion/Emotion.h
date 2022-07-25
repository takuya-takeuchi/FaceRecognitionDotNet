#ifndef _CPP_LOSS_MULTICLASS_LOG_EMOTION_H_
#define _CPP_LOSS_MULTICLASS_LOG_EMOTION_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

#include "DlibDotNet.Native/dlib/export.h"
#include "DlibDotNet.Native/dlib/shared.h"
#include "defines.h"
#include "DlibDotNet.Native.Dnn/dlib/dnn/loss/multiclass_log/template.h"

typedef uint32_t emotion_out_type;
typedef uint32_t emotion_train_label_type;

MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type,  matrix_element_type::Double,   double,          matrix_element_type::UInt32, emotion_train_label_type, 200)
MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type2, matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, emotion_train_label_type, 201)
MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type3, matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, emotion_train_label_type, 202)
MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type4, matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, emotion_train_label_type, 203)
MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type5, matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, emotion_train_label_type, 204)
MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type6, matrix_element_type::UInt8,    uint8_t,         matrix_element_type::UInt32, emotion_train_label_type, 205)

DLLEXPORT void LossMulticlassLog_emotion_train_type_eval(int32_t id, void* obj)
{
    switch(id)
    {
        case 200:
            {
                auto& net = *static_cast<emotion_train_type*>(obj);
                dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
                dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
            }
            break;
        // case 201:
        //     {
        //         auto& net = *static_cast<emotion_train_type2*>(obj);
        //         dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
        //         dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
        //     }
        //     break;
        case 202:
            {
                auto& net = *static_cast<emotion_train_type3*>(obj);
                dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
            }
            break;
        case 203:
            {
                auto& net = *static_cast<emotion_train_type4*>(obj);
                dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
                dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
            }
            break;
    }
}

#endif