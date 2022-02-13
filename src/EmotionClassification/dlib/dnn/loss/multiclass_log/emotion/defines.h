#ifndef _CPP_LOSS_MULTICLASS_LOG_DEFINES_EMOTION_DEFINES_H_
#define _CPP_LOSS_MULTICLASS_LOG_DEFINES_EMOTION_DEFINES_H_

#include <dlib/dnn.h>
#include <dlib/matrix.h>

using namespace dlib;
using namespace std;

// Developer can customize these as you want to do!!!
#pragma region type definitions

// avoid to conflict with previous declaration
namespace emotion
{
	// https://arxiv.org/pdf/1812.04510.pdf
	template <typename SUBNET>
	using fc1 = add_layer<fc_<100, FC_HAS_BIAS>, SUBNET>;
	template <typename SUBNET>
	using fc2 = add_layer<fc_<500, FC_HAS_BIAS>, SUBNET>;
	template <typename SUBNET>
	using fc3 = add_layer<fc_<8, FC_HAS_BIAS>, SUBNET>;
}

using emotion_train_type = loss_multiclass_log<emotion::fc3<dropout<
                                               sig<emotion::fc2<dropout<
                                               sig<emotion::fc1<input<matrix<double>>>>>>>>>>;

// avoid to conflict with previous declaration
namespace emotion
{
// Inception layer has some different convolutions inside.  Here we define
// blocks as convolutions with different kernel size that we will use in
// inception layer block.
template <typename SUBNET> using block_a1 = relu<con<10,1,1,1,1,SUBNET>>;
template <typename SUBNET> using block_a2 = relu<con<10,3,3,1,1,relu<con<16,1,1,1,1,SUBNET>>>>;
template <typename SUBNET> using block_a3 = relu<con<10,5,5,1,1,relu<con<16,1,1,1,1,SUBNET>>>>;
template <typename SUBNET> using block_a4 = relu<con<10,1,1,1,1,max_pool<3,3,1,1,SUBNET>>>;

// Here is inception layer definition. It uses different blocks to process input
// and returns combined output.  Dlib includes a number of these inceptionN
// layer types which are themselves created using concat layers.  
template <typename SUBNET> using incept_a = inception4<emotion::block_a1,emotion::block_a2,emotion::block_a3,emotion::block_a4, SUBNET>;

// Network can have inception layers of different structure.  It will work
// properly so long as all the sub-blocks inside a particular inception block
// output tensors with the same number of rows and columns.
template <typename SUBNET> using block_b1 = relu<con<4,1,1,1,1,SUBNET>>;
template <typename SUBNET> using block_b2 = relu<con<4,3,3,1,1,SUBNET>>;
template <typename SUBNET> using block_b3 = relu<con<4,1,1,1,1,max_pool<3,3,1,1,SUBNET>>>;
template <typename SUBNET> using incept_b = inception3<emotion::block_b1,emotion::block_b2,emotion::block_b3,SUBNET>;
}

// It occurs overfit to training data
using emotion_train_type2 = loss_multiclass_log<
        fc<8,
        relu<fc<32,
        max_pool<2,2,2,2,emotion::incept_b<
        max_pool<2,2,2,2,emotion::incept_a<
        input_rgb_image_sized<227>
        >>>>>>>>;

using emotion_train_type3 = loss_multiclass_log<
        fc<8,
        relu<fc<32,
        max_pool<2,2,2,2,dropout<emotion::incept_b<
        max_pool<2,2,2,2,emotion::incept_a<
        input_rgb_image_sized<227>
        >>>>>>>>>;

// alexnet
// https://www.researchgate.net/publication/333159395_Human_Emotion_Recognition_in_Video_Using_Subtraction_Pre-Processing
using emotion_train_type4 = loss_multiclass_log<
	   fc<8, 
       dropout<relu<fc<4096, 
       dropout<relu<fc<4096, 
       max_pool<3, 3, 2, 2, relu<con<256, 3, 3, 1, 1,  
       relu<con<384, 3, 3, 1, 1, 
       relu<con<384, 3, 3, 1, 1,
       max_pool<3, 3, 2, 2, l2normalize<relu<con<256, 5, 5, 1, 1, 
       max_pool<3, 3, 2, 2, l2normalize<relu<con<96, 11, 11, 1, 1, 
       input_rgb_image_sized<227>>>>>>>>>>>>>>>>>>>>>>>>;

// avoid to conflict with previous declaration
namespace emotion
{
// restnet
// https://www.researchgate.net/publication/333159395_Human_Emotion_Recognition_in_Video_Using_Subtraction_Pre-Processing	   
template <template <int,template<typename>class,int,typename> class block, int N, template<typename>class BN, typename SUBNET>
using residual = add_prev1<block<N,BN,1,tag1<SUBNET>>>;

template <template <int,template<typename>class,int,typename> class block, int N, template<typename>class BN, typename SUBNET>
using residual_down = add_prev2<avg_pool<2,2,2,2,skip1<tag2<block<N,BN,2,tag1<SUBNET>>>>>>;

template <int N, template <typename> class BN, int stride, typename SUBNET> 
using block = BN<con<N,3,3,1,1,relu<BN<con<N,3,3,stride,stride,SUBNET>>>>>;

template <int N, typename SUBNET> using ares      = relu<emotion::residual<emotion::block,N,affine,SUBNET>>;
template <int N, typename SUBNET> using ares_down = relu<emotion::residual_down<emotion::block,N,affine,SUBNET>>;

template <typename SUBNET> using resnet_34_level1 = emotion::ares<512,emotion::ares<512,emotion::ares_down<512,SUBNET>>>;
template <typename SUBNET> using resnet_34_level2 = emotion::ares<256,emotion::ares<256,emotion::ares<256,emotion::ares<256,emotion::ares<256,emotion::ares_down<256,SUBNET>>>>>>;
template <typename SUBNET> using resnet_34_level3 = emotion::ares<128,emotion::ares<128,emotion::ares<128,emotion::ares_down<128,SUBNET>>>>;
template <typename SUBNET> using resnet_34_level4 = emotion::ares<64, emotion::ares<64, emotion::ares<64, SUBNET>>>;

template <typename SUBNET> using resnet_18_level1 = emotion::ares<512,emotion::ares_down<512,SUBNET>>;
template <typename SUBNET> using resnet_18_level2 = emotion::ares<256,emotion::ares_down<256,SUBNET>>;
template <typename SUBNET> using resnet_18_level3 = emotion::ares<128,emotion::ares_down<128,SUBNET>>;
template <typename SUBNET> using resnet_18_level4 = emotion::ares<64, emotion::ares<64,SUBNET>>;

template <typename SUBNET> using resnet_10_level1 = emotion::ares_down<512,SUBNET>;
template <typename SUBNET> using resnet_10_level2 = emotion::ares_down<256,SUBNET>;
template <typename SUBNET> using resnet_10_level3 = emotion::ares_down<128,SUBNET>;
template <typename SUBNET> using resnet_10_level4 = emotion::ares<64,SUBNET>;
}

// restnet-34
// using emotion_train_type5 = loss_multiclass_log<fc<8,avg_pool_everything<
//                             resnet_34_level1<
//                             resnet_34_level2<
//                             resnet_34_level3<
//                             resnet_34_level4<
//                             max_pool<3,3,2,2,relu<affine<con<64,7,7,2,2,
//                             input_rgb_image_sized<227>
//                             >>>>>>>>>>>;
// restnet-18
// using emotion_train_type5 = loss_multiclass_log<fc<8,avg_pool_everything<
//                             resnet_18_level1<
//                             resnet_18_level2<
//                             resnet_18_level3<
//                             resnet_18_level4<
//                             max_pool<3,3,2,2,relu<affine<con<64,7,7,2,2,
//                             input_rgb_image_sized<227>
//                             >>>>>>>>>>>;
// restnet-10
using emotion_train_type5 = loss_multiclass_log<fc<8,avg_pool_everything<
                            emotion::resnet_10_level1<
                            emotion::resnet_10_level2<
                            emotion::resnet_10_level3<
                            emotion::resnet_10_level4<
                            max_pool<3,3,2,2,relu<affine<con<64,7,7,2,2,
                            input_rgb_image_sized<227>
                            >>>>>>>>>>>;
// resnet-10 for grayscale
using emotion_train_type6 = loss_multiclass_log<fc<8,avg_pool_everything<
                            emotion::resnet_10_level1<
                            emotion::resnet_10_level2<
                            emotion::resnet_10_level3<
                            emotion::resnet_10_level4<
                            max_pool<3,3,2,2,relu<affine<con<64,7,7,2,2,
                            input<matrix<uint8_t>>
                            >>>>>>>>>>>;

static const std::vector<const char *>* emotion_train_type_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

static const std::vector<const char *>* emotion_train_type2_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

static const std::vector<const char *>* emotion_train_type3_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

static const std::vector<const char *>* emotion_train_type4_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

static const std::vector<const char *>* emotion_train_type5_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

static const std::vector<const char *>* emotion_train_type6_labels = new std::vector<const char *>(
{
    "anger", 
	"contempt", 
	"disgust", 
	"fear", 
	"happiness", 
	"neutrality", 
	"sadness", 
	"surprise"
});

#pragma endregion type definitions

#endif