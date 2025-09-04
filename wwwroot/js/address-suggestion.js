class AddressSuggestion {
    constructor() {
        this.provinceSelect = null;
        this.districtSelect = null;
        this.wardSelect = null;
        this.addressInput = null;
        this.init();
    }

    init() {
        console.log('DEBUG: Initializing AddressSuggestion...');
        
        // Tìm các element
        this.provinceSelect = document.getElementById('province-select');
        this.districtSelect = document.getElementById('district-select');
        this.wardSelect = document.getElementById('ward-select');
        this.addressInput = document.getElementById('address-input');

        console.log('DEBUG: Elements found:', {
            provinceSelect: !!this.provinceSelect,
            districtSelect: !!this.districtSelect,
            wardSelect: !!this.wardSelect,
            addressInput: !!this.addressInput
        });

        if (this.provinceSelect) {
            console.log('DEBUG: Province select found, loading provinces and setting up listeners...');
            this.loadProvinces();
            this.setupEventListeners();
        } else {
            console.error('DEBUG: Province select not found! Make sure elements exist in DOM.');
        }
    }

    setupEventListeners() {
        if (this.provinceSelect) {
            this.provinceSelect.addEventListener('change', () => {
                this.onProvinceChange();
            });
        }

        if (this.districtSelect) {
            this.districtSelect.addEventListener('change', () => {
                this.onDistrictChange();
            });
        }

        if (this.wardSelect) {
            this.wardSelect.addEventListener('change', () => {
                this.onWardChange();
            });
        }

        // Auto-complete cho địa chỉ
        if (this.addressInput) {
            this.addressInput.addEventListener('input', (e) => {
                this.suggestAddress(e.target.value);
            });
            
            // Thêm event listener để cập nhật shipping info khi thay đổi địa chỉ
            this.addressInput.addEventListener('blur', async () => {
                await this.updateShippingInfo();
            });
        }

        // Button lưu địa chỉ
        const saveAddressBtn = document.getElementById('save-address-btn');
        if (saveAddressBtn) {
            saveAddressBtn.addEventListener('click', async () => {
                console.log('DEBUG: Button lưu địa chỉ được click');
                await this.updateShippingInfo();
            });
        }
    }

    async loadProvinces() {
        try {
            console.log('DEBUG: Đang tải danh sách tỉnh/thành phố...');
            const response = await fetch('/Cart/GetProvinces');
            console.log('DEBUG: Response status:', response.status);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const provinces = await response.json();
            console.log('DEBUG: Provinces loaded:', provinces.length);
            
            this.provinceSelect.innerHTML = '<option value="">Chọn Tỉnh/Thành phố</option>';
            provinces.forEach(province => {
                const option = document.createElement('option');
                option.value = province.code;
                option.textContent = province.name;
                this.provinceSelect.appendChild(option);
            });
            
            console.log('DEBUG: Provinces dropdown updated successfully');
        } catch (error) {
            console.error('Lỗi khi tải danh sách tỉnh/thành phố:', error);
            // Fallback: Thêm một số tỉnh phổ biến
            this.provinceSelect.innerHTML = `
                <option value="">Chọn Tỉnh/Thành phố</option>
                <option value="01">Hà Nội</option>
                <option value="79">TP. Hồ Chí Minh</option>
                <option value="48">Đà Nẵng</option>
                <option value="31">Hải Phòng</option>
                <option value="92">Cần Thơ</option>
            `;
        }
    }

    async onProvinceChange() {
        const provinceCode = this.provinceSelect.value;
        console.log('DEBUG: Province changed to:', provinceCode);
        
        // Reset district và ward
        this.districtSelect.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
        this.wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
        
        if (!provinceCode) return;

        try {
            console.log(`DEBUG: Loading districts for province: ${provinceCode}`);
            const response = await fetch(`/Cart/GetDistricts?provinceCode=${provinceCode}`);
            console.log('DEBUG: Districts response status:', response.status);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const districts = await response.json();
            console.log('DEBUG: Districts loaded:', districts.length);
            
            if (districts.length === 0) {
                console.log('DEBUG: No districts found, adding fallback options');
                // Fallback cho các tỉnh phổ biến
                if (provinceCode === '01') { // Hà Nội
                    this.districtSelect.innerHTML += `
                        <option value="001">Ba Đình</option>
                        <option value="002">Hoàn Kiếm</option>
                        <option value="005">Cầu Giấy</option>
                        <option value="006">Đống Đa</option>
                        <option value="009">Thanh Xuân</option>
                    `;
                } else if (provinceCode === '79') { // TP. HCM
                    this.districtSelect.innerHTML += `
                        <option value="760">Quận 1</option>
                        <option value="761">Quận 12</option>
                        <option value="765">Quận Bình Thạnh</option>
                        <option value="766">Quận Tân Bình</option>
                        <option value="767">Quận Tân Phú</option>
                    `;
                }
            } else {
                districts.forEach(district => {
                    const option = document.createElement('option');
                    option.value = district.code;
                    option.textContent = district.name;
                    this.districtSelect.appendChild(option);
                });
            }
            
            console.log('DEBUG: Districts dropdown updated successfully');
        } catch (error) {
            console.error('Lỗi khi tải danh sách quận/huyện:', error);
        }
    }

    async onDistrictChange() {
        const provinceCode = this.provinceSelect.value;
        const districtCode = this.districtSelect.value;
        console.log(`DEBUG: District changed to: ${districtCode} (Province: ${provinceCode})`);
        
        // Reset ward
        this.wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';
        
        if (!provinceCode || !districtCode) return;

        try {
            console.log(`DEBUG: Loading wards for district: ${districtCode}, province: ${provinceCode}`);
            const response = await fetch(`/Cart/GetWards?provinceCode=${provinceCode}&districtCode=${districtCode}`);
            console.log('DEBUG: Wards response status:', response.status);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const wards = await response.json();
            console.log('DEBUG: Wards loaded:', wards.length);
            
            if (wards.length === 0) {
                console.log('DEBUG: No wards found, adding fallback options');
                // Fallback: Thêm một số phường/xã mẫu
                this.wardSelect.innerHTML += `
                    <option value="00001">Phường 1</option>
                    <option value="00002">Phường 2</option>
                    <option value="00003">Phường 3</option>
                    <option value="00004">Phường 4</option>
                    <option value="00005">Phường 5</option>
                `;
            } else {
                wards.forEach(ward => {
                    const option = document.createElement('option');
                    option.value = ward.code;
                    option.textContent = ward.name;
                    this.wardSelect.appendChild(option);
                });
            }
            
            console.log('DEBUG: Wards dropdown updated successfully');
        } catch (error) {
            console.error('Lỗi khi tải danh sách phường/xã:', error);
            // Fallback khi có lỗi
            this.wardSelect.innerHTML += `
                <option value="00001">Phường 1</option>
                <option value="00002">Phường 2</option>
                <option value="00003">Phường 3</option>
            `;
        }
    }

    async onWardChange() {
        // Cập nhật thông tin vận chuyển khi chọn phường/xã
        await this.updateShippingInfo();
    }

    suggestAddress(input) {
        // Gợi ý địa chỉ dựa trên input
        const suggestions = [
            'Số nhà, tên đường',
            'Tên chung cư, số phòng',
            'Tên tòa nhà, tầng',
            'Địa chỉ công ty',
            'Địa chỉ trường học'
        ];

        // Hiển thị gợi ý nếu input rỗng hoặc ngắn
        if (input.length < 3) {
            this.showAddressSuggestions(suggestions);
        } else {
            this.hideAddressSuggestions();
        }
    }

    showAddressSuggestions(suggestions) {
        let suggestionBox = document.getElementById('address-suggestions');
        if (!suggestionBox) {
            suggestionBox = document.createElement('div');
            suggestionBox.id = 'address-suggestions';
            suggestionBox.className = 'address-suggestions';
            suggestionBox.style.cssText = `
                position: absolute;
                top: 100%;
                left: 0;
                right: 0;
                background: white;
                border: 1px solid #ddd;
                border-top: none;
                border-radius: 0 0 4px 4px;
                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                z-index: 1000;
                max-height: 200px;
                overflow-y: auto;
            `;
            
            const addressContainer = this.addressInput.parentElement;
            addressContainer.style.position = 'relative';
            addressContainer.appendChild(suggestionBox);
        }

        suggestionBox.innerHTML = suggestions.map(suggestion => 
            `<div class="suggestion-item" style="padding: 8px 12px; cursor: pointer; border-bottom: 1px solid #eee;" onclick="addressSuggestion.selectSuggestion('${suggestion}')">${suggestion}</div>`
        ).join('');
        
        suggestionBox.style.display = 'block';
    }

    hideAddressSuggestions() {
        const suggestionBox = document.getElementById('address-suggestions');
        if (suggestionBox) {
            suggestionBox.style.display = 'none';
        }
    }

    async selectSuggestion(suggestion) {
        this.addressInput.value = suggestion;
        this.hideAddressSuggestions();
        await this.updateShippingInfo();
    }

    async updateShippingInfo() {
        // Cập nhật thông tin vận chuyển khi thay đổi địa chỉ
        const province = this.provinceSelect.options[this.provinceSelect.selectedIndex]?.text || '';
        const district = this.districtSelect.options[this.districtSelect.selectedIndex]?.text || '';
        const ward = this.wardSelect.options[this.wardSelect.selectedIndex]?.text || '';
        const address = this.addressInput.value || '';

        // Hiển thị địa chỉ đã chọn
        const selectedAddress = document.getElementById('selected-address');
        if (selectedAddress) {
            if (province && district && ward && address) {
                selectedAddress.innerHTML = `
                    <div style="background: #f8f9fa; padding: 10px; border-radius: 4px; margin-top: 10px;">
                        <strong>Địa chỉ đã chọn:</strong><br>
                        ${address}<br>
                        ${ward}, ${district}, ${province}
                    </div>
                `;
                selectedAddress.style.display = 'block';
                
                // Gọi API để lưu thông tin vận chuyển
                await this.saveShippingInfo(province, district, ward, address);
            } else {
                selectedAddress.style.display = 'none';
            }
        }
    }

    async saveShippingInfo(province, district, ward, address) {
        try {
            console.log('DEBUG: Gọi API GetShipping với thông tin:', { province, district, ward, address });
            
            // Kiểm tra xem có đủ thông tin không
            if (!province || !district || !ward || !address) {
                console.log('DEBUG: Thiếu thông tin địa chỉ, không gọi API');
                return;
            }
            
            const response = await fetch('/Cart/GetShipping', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: new URLSearchParams({
                    tinh: province,
                    quan: district,
                    phuong: ward,
                    address: address
                })
            });

            console.log('DEBUG: Response status:', response.status);
            console.log('DEBUG: Response headers:', response.headers);

            if (response.ok) {
                const result = await response.json();
                console.log('DEBUG: API GetShipping response:', result);
                
                if (result.success) {
                    // Cập nhật giá vận chuyển và tổng tiền
                    this.updateShippingPrice(result.shippingPrice, result.finalTotal);
                    console.log('DEBUG: Đã lưu thông tin vận chuyển thành công');
                } else {
                    console.error('DEBUG: API GetShipping failed:', result.message);
                }
            } else {
                const errorText = await response.text();
                console.error('DEBUG: API GetShipping HTTP error:', response.status, errorText);
            }
        } catch (error) {
            console.error('DEBUG: Lỗi khi gọi API GetShipping:', error);
        }
    }

    updateShippingPrice(shippingPrice, finalTotal) {
        // Cập nhật hiển thị giá vận chuyển và tổng tiền
        const shippingPriceElement = document.getElementById('shipping-price');
        const finalTotalElement = document.getElementById('final-total');
        
        if (shippingPriceElement) {
            shippingPriceElement.textContent = shippingPrice.toLocaleString('vi-VN') + '₫';
        }
        
        if (finalTotalElement) {
            finalTotalElement.textContent = finalTotal.toLocaleString('vi-VN') + '₫';
        }
        
        console.log('DEBUG: Đã cập nhật giá vận chuyển:', shippingPrice, 'và tổng tiền:', finalTotal);
    }

    // Lấy thông tin địa chỉ đầy đủ
    getFullAddress() {
        const province = this.provinceSelect.options[this.provinceSelect.selectedIndex]?.text || '';
        const district = this.districtSelect.options[this.districtSelect.selectedIndex]?.text || '';
        const ward = this.wardSelect.options[this.wardSelect.selectedIndex]?.text || '';
        const address = this.addressInput.value || '';

        return {
            province,
            district,
            ward,
            address,
            fullAddress: `${address}, ${ward}, ${district}, ${province}`.replace(/^,\s*/, '').replace(/,\s*,/g, ',')
        };
    }

    // Validate địa chỉ
    validateAddress() {
        const province = this.provinceSelect.value;
        const district = this.districtSelect.value;
        const ward = this.wardSelect.value;
        const address = this.addressInput.value.trim();

        const errors = [];

        if (!province) errors.push('Vui lòng chọn Tỉnh/Thành phố');
        if (!district) errors.push('Vui lòng chọn Quận/Huyện');
        if (!ward) errors.push('Vui lòng chọn Phường/Xã');
        if (!address) errors.push('Vui lòng nhập địa chỉ chi tiết');

        return {
            isValid: errors.length === 0,
            errors
        };
    }
}

// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function() {
    window.addressSuggestion = new AddressSuggestion();
});

// Export cho sử dụng global
window.AddressSuggestion = AddressSuggestion;
