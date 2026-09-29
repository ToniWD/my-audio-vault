import { useState } from 'react';
import {
    View,
    Text,
    TextInput,
    TouchableOpacity,
    ActivityIndicator,
    KeyboardAvoidingView,
    ScrollView,
    Platform
} from 'react-native';
import { useModal } from '@/components/modal/ModalContext';
import { ProblemDetails, putApiUsersMePassword } from '@/api';
import { handleApiError } from '@/utils/helperApiError';
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';
import { useAuth } from '@/hooks/useAuth';

export const ChangePasswordScreen = () => {
    const { showModal } = useModal();
    const { logout } = useAuth();

    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);

    const isValid = () => {
        return (
            currentPassword.trim() !== '' &&
            newPassword.trim() !== '' &&
            confirmPassword.trim() !== ''
        );
    };

    async function handleChangePassword() {
        if (newPassword !== confirmPassword) {
            showModal({
                title: 'Validation Error',
                message: 'New password and confirmation password do not match.',
                acceptLabel: 'OK'
            });
            return;
        }

        setLoading(true);

        try {
            const result = await putApiUsersMePassword({
                body: {
                    currentPassword: currentPassword,
                    newPassword: newPassword
                }
            });

            if (result.error) {
                console.error(result.error);
                handleApiError(result.error as ProblemDetails, showModal);
            } else {
                showModal({
                    title: 'Success',
                    message: 'Your password has been changed successfully.',
                    acceptLabel: 'OK',
                    onAccept: () => logout()
                });
            }
        } catch (err) {
            console.error('Change password error:', err);
        } finally {
            setLoading(false);
        }
    }

    return (
        <KeyboardAvoidingView
            style={globalStyles.screen}
            behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        >
            <ScrollView
                contentContainerStyle={globalStyles.scrollContent}
                keyboardShouldPersistTaps="handled"
            >
                <View style={globalStyles.formGroup}>
                    <Text style={globalStyles.title}>Change Password</Text>
                    <Text style={globalStyles.subtitle}>Enter your current password and set a new one</Text>

                    <Text style={globalStyles.label}>Current Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={currentPassword}
                        onChangeText={setCurrentPassword}
                        placeholder="Enter current password"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                        autoCapitalize="none"
                    />

                    <Text style={globalStyles.label}>New Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={newPassword}
                        onChangeText={setNewPassword}
                        placeholder="Enter new password"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                        autoCapitalize="none"
                    />

                    <Text style={globalStyles.label}>Confirm New Password</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={confirmPassword}
                        onChangeText={setConfirmPassword}
                        placeholder="Confirm new password"
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        secureTextEntry
                        autoCapitalize="none"
                    />

                    <TouchableOpacity
                        style={[
                            globalStyles.button,
                            (loading || !isValid()) && globalStyles.buttonDisabled
                        ]}
                        onPress={handleChangePassword}
                        disabled={loading || !isValid()}
                    >
                        {loading ? (
                            <ActivityIndicator color={Colors.mainTheme.borderActive} />
                        ) : (
                            <Text style={globalStyles.buttonText}>Update Password</Text>
                        )}
                    </TouchableOpacity>
                </View>
            </ScrollView>
        </KeyboardAvoidingView>
    );
};

export default ChangePasswordScreen;