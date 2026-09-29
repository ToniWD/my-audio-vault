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
import { patchApiUsersMe, ProblemDetails } from '@/api';
import { handleApiError } from '@/utils/helperApiError';
import { globalStyles } from '@/constants/globalStyles';
import { Colors } from '@/constants/colors';
import { useAuth } from '@/hooks/useAuth';
import { useSafeBack } from '@/hooks/useSafeBack';

export const EditProfileScreen = () => {
    const { user, fetchUser } = useAuth();
    const { showModal } = useModal();
    const { goBackOrFallback } = useSafeBack();

    const [username, setUsername] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);

    const isModified = () => {
        return (
            (username.trim() !== '' && username.trim() !== user?.username) ||
            (firstName.trim() !== '' && firstName.trim() !== user?.firstName) ||
            (lastName.trim() !== '' && lastName.trim() !== user?.lastName) ||
            (email.trim() !== '' && email.trim() !== user?.email)
        );
    };

    async function handelPatchProfile() {
        setLoading(true);

        try {
            const result = await patchApiUsersMe({
                body: {
                    username: username.trim() || null,
                    firstName: firstName.trim() || null,
                    lastName: lastName.trim() || null,
                    email: email.trim() || null
                }
            });

            if (result.error) {
                console.error(result.error);
                handleApiError(result.error as ProblemDetails, showModal);
            } else {
                await fetchUser();
                goBackOrFallback("/profile")
            }
        } catch (err) {
            console.error('Patch profile error:', err);
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
                    <Text style={globalStyles.title}>Edit Profile</Text>
                    <Text style={globalStyles.subtitle}>Update your personal information</Text>

                    <Text style={globalStyles.label}>Username</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={username}
                        onChangeText={setUsername}
                        placeholder={user?.username || 'Username'}
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        autoCapitalize="none"
                    />

                    <View style={globalStyles.row}>
                        <View style={globalStyles.flex1}>
                            <Text style={globalStyles.label}>First Name</Text>
                            <TextInput
                                style={globalStyles.input}
                                value={firstName}
                                onChangeText={setFirstName}
                                placeholder={user?.firstName || 'First Name'}
                                placeholderTextColor={Colors.mainTheme.textSecondary}
                                autoCapitalize="words"
                            />
                        </View>
                        <View style={globalStyles.flex1}>
                            <Text style={globalStyles.label}>Last Name</Text>
                            <TextInput
                                style={globalStyles.input}
                                value={lastName}
                                onChangeText={setLastName}
                                placeholder={user?.lastName || 'Last Name'}
                                placeholderTextColor={Colors.mainTheme.textSecondary}
                                autoCapitalize="words"
                            />
                        </View>
                    </View>

                    <Text style={globalStyles.label}>Email</Text>
                    <TextInput
                        style={globalStyles.input}
                        value={email}
                        onChangeText={setEmail}
                        placeholder={user?.email || 'Email'}
                        placeholderTextColor={Colors.mainTheme.textSecondary}
                        autoCapitalize="none"
                        keyboardType="email-address"
                        autoComplete="email"
                    />

                    <TouchableOpacity
                        style={[
                            globalStyles.button,
                            (loading || !isModified()) && globalStyles.buttonDisabled
                        ]}
                        onPress={handelPatchProfile}
                        disabled={loading || !isModified()}
                    >
                        {loading ? (
                            <ActivityIndicator color={Colors.mainTheme.borderActive} />
                        ) : (
                            <Text style={globalStyles.buttonText}>Save</Text>
                        )}
                    </TouchableOpacity>
                </View>
            </ScrollView>
        </KeyboardAvoidingView>
    );
};

export default EditProfileScreen;