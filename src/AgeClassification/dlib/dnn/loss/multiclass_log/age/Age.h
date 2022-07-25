#ifndef _CPP_LOSS_MULTICLASS_LOG_AGE_H_
#define _CPP_LOSS_MULTICLASS_LOG_AGE_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

#include "DlibDotNet.Native/dlib/export.h"
#include "DlibDotNet.Native/dlib/shared.h"
#include "defines.h"
#include "DlibDotNet.Native.Dnn/dlib/dnn/loss/multiclass_log/template.h"

typedef uint32_t age_out_type;
typedef uint32_t age_train_label_type;

MAKE_LOSSMULTICLASSLOG_FUNC(age_train_type, matrix_element_type::RgbPixel, dlib::rgb_pixel, matrix_element_type::UInt32, age_train_label_type, 200)

DLLEXPORT void LossMulticlassLog_age_train_type_eval(void* obj)
{
    auto& net = *static_cast<age_train_type*>(obj);
    dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
    dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
}

#endif