#pragma once
class VendorData
{
public:
	virtual int GetVendorID() = 0;
	virtual ~VendorData();
};

class ACPIVendorData : public VendorData
{
public:
	virtual int GetVendorID() override;
};

