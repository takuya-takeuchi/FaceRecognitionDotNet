#ifndef _CPP_LOSS_MULTICLASS_LOG_EMOTION_H_
#define _CPP_LOSS_MULTICLASS_LOG_EMOTION_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

#include "DlibDotNet.Native/dlib/export.h"
#include "DlibDotNet.Native/dlib/shared.h"
#include "defines.h"
#include "DlibDotNet.Native.Dnn/dlib/dnn/loss/multiclass_log/template.h"

typedef unsigned long emotion_out_type;
typedef unsigned long emotion_train_label_type;

MAKE_LOSSMULTICLASSLOG_FUNC(emotion_train_type, matrix_element_type::Double, double, matrix_element_type::UInt32, emotion_train_label_type, 200)

DLLEXPORT void LossMulticlassLog_emotion_train_type_eval(void* obj)
{
    auto& net = *static_cast<emotion_train_type*>(obj);
    // dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
    // dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
}

DLLEXPORT void LossMulticlassLog_emotion_train_type_test(std::vector<dlib::matrix<double>*>* const data)
{
    printf("%d", data->size());
    // dlib::layer<2>(net).layer_details() = dlib::dropout_(0);
    // dlib::layer<5>(net).layer_details() = dlib::dropout_(0);
}

#endif