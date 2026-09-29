import React, { useEffect, useState } from 'react';
import { Modal, View, Text, TouchableOpacity, TextInput } from 'react-native';
import { ModalConfig } from './ModalContext';
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';

interface GlobalModalProps {
    config: ModalConfig;
    onAccept: (inputValue?: string) => void;
    onRefuse: () => void;
}

export const GlobalModalPopup: React.FC<GlobalModalProps> = ({
    config,
    onAccept,
    onRefuse,
}) => {
    const [inputValue, setInputValue] = useState('');

    useEffect(() => {
        setInputValue('');
    }, [config]);

    const handleAccept = () => {
        console.log(config.hasInput + " <=> " + inputValue)
        onAccept(config.hasInput ? inputValue : undefined);
    };

    return (
        <Modal transparent animationType="fade" visible={true}>
            <View style={globalStyles.overlay}>
                <View style={globalStyles.dialog}>
                    {config.title && <Text style={globalStyles.title}>{config.title}</Text>}
                    <Text style={globalStyles.dialogText}>{config.message}</Text>

                    {config.hasInput && (
                        <TextInput
                            style={globalStyles.input}
                            placeholder={config.inputPlaceholder || 'Type here...'}
                            placeholderTextColor={Colors.mainTheme.textSecondary}
                            value={inputValue}
                            onChangeText={setInputValue}
                            secureTextEntry={config.secureTextEntry}
                            autoCapitalize="none"
                        />
                    )}

                    <View style={globalStyles.buttonRow}>
                        {(config.refuseLabel || config.onRefuse != undefined) && <TouchableOpacity
                            style={globalStyles.button}
                            onPress={onRefuse}
                        >
                            <Text style={globalStyles.buttonTextDanger}>
                                {config.refuseLabel || 'Cancel'}
                            </Text>
                        </TouchableOpacity>
                        }

                        <TouchableOpacity
                            style={[globalStyles.button, globalStyles.buttonPrimary]}
                            onPress={handleAccept}
                        >
                            <Text style={globalStyles.buttonText}>
                                {config.acceptLabel || 'Confirm'}
                            </Text>
                        </TouchableOpacity>
                    </View>
                </View>
            </View>
        </Modal>
    );
};
